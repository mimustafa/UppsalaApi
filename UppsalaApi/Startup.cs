using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Formatters;
using UppsalaApi.Infrastructure;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using UppsalaApi.Filters;
using Microsoft.EntityFrameworkCore;
using UppsalaApi.Models;
using UppsalaApi.Services;
using AutoMapper;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using AspNet.Security.OpenIdConnect.Primitives;

namespace UppsalaApi
{
    public class Startup
    {
        private readonly int? _httpsPort;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            //Get HTTPS port (only in development)
            if(env.IsDevelopment())
            {
                var launchJsonConfig = new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("Properties//launchSettings.json")
                    .Build();
                _httpsPort = launchJsonConfig.GetValue<int>("iisSettings:iisExpress:sslPort");
            }
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // use inMemory Database fro quick dev and testing
            //TODO: Swap out with a real database in production
            services.AddDbContext<UppsalaApiContext>(opt =>
            {
                opt.UseInMemoryDatabase();
                opt.UseOpenIddict<Guid>();
            });

            // Map some of the default claim names to the proper OpenID Connect claim names
            services.Configure<IdentityOptions>(opt =>
            {
                opt.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                opt.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                opt.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            //// Add OpenIddict services
            //services.AddOpenIddict().AddCore(opt =>
            //{
            //    opt.AddEntityFrameworkCoreStores<UppsalaApiContext>();
            //    opt.AddMvcBinders();
            //    opt.EnableTokenEndpoint("/token");
            //    opt.AllowPasswordFlow();
            //});

            // Register the OpenIddict services.
            services.AddOpenIddict()
                .AddCore(opt =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and entities.
                    opt.UseEntityFrameworkCore()
                       .UseDbContext<UppsalaApiContext>()
                       .ReplaceDefaultEntities<Guid>();
                })
                .AddServer(opt =>
                {
                    // Register the ASP.NET Core MVC binder used by OpenIddict.
                    // Note: if you don't call this method, you won't be able to
                    // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                    opt.UseMvc();

                    // Enable the token endpoint (required to use the password flow).
                    opt.EnableTokenEndpoint("/token");

                    // Allow client applications to use the grant_type=password flow.
                    opt.AllowPasswordFlow();

                    // During development, you can disable the HTTPS requirement.
                    //opt.DisableHttpsRequirement();

                    // Accept token requests that don't specify a client_id.
                    opt.AcceptAnonymousClients();
                });
                //.AddValidation();

             

            // Add ASP.NET Core Identity
            services.AddIdentity<UserEntity, UserRoleEntity>()
                .AddEntityFrameworkStores<UppsalaApiContext, Guid>()
                .AddDefaultTokenProviders();

            services.AddAutoMapper();
            services.AddResponseCaching();

            // Add framework services.
            services.AddMvc( opt => 
            {
                opt.Filters.Add(typeof(JsonExecptionFilter));
                opt.Filters.Add(typeof(LinkRewritingFilter));

                // Require HTTPS for all controllers
                opt.SslPort = _httpsPort;
                opt.Filters.Add(typeof(RequireHttpsAttribute));

                var jsonFormatter = opt.OutputFormatters.OfType<JsonOutputFormatter>().Single();
                opt.OutputFormatters.Remove(jsonFormatter);
                opt.OutputFormatters.Add(new IonOutputFormatter(jsonFormatter));

                opt.CacheProfiles.Add("Static", new CacheProfile { Duration = 86400 });
                opt.CacheProfiles.Add("Collection", new CacheProfile { Duration = 60 });
                opt.CacheProfiles.Add("Resource", new CacheProfile { Duration = 180 });
            })

             .AddJsonOptions(opt =>
              {
                  // These should be the defaults, but we can be explicit:
                  opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                  opt.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                  opt.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
              });


            services.AddRouting(opt=> opt.LowercaseUrls = true);

            services.AddApiVersioning(opt =>
            {
                opt.ApiVersionReader = new MediaTypeApiVersionReader();
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.ReportApiVersions = true;
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.ApiVersionSelector = new CurrentImplementationApiVersionSelector(opt);
            });

            services.Configure<CampusOptions>(Configuration);
            services.Configure<CampusInfo>(Configuration.GetSection("Info"));
            services.Configure<PagingOptions>(Configuration.GetSection("DefaultPagingOptions"));

            services.AddScoped<IRoomService, DefaultRoomService>();
            services.AddScoped<IOpeningService, DefaultOpeningService>();
            services.AddScoped<IBookingService, DefaultBookingService>();
            services.AddScoped<IDateLogicService, DefaultDateLogicService>();
            services.AddScoped<IUserService, DefaultUserService>();

            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("ViewAllUsersPolicy",
                    p => p.RequireAuthenticatedUser().RequireRole("Admin"));

                opt.AddPolicy("ViewAllBookingsPolicy",
                    p => p.RequireAuthenticatedUser().RequireRole("Admin"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Add some test data in development
            if(env.IsDevelopment())
            {
                // Add test roles and users
                var roleManager = app.ApplicationServices
                    .GetRequiredService<RoleManager<UserRoleEntity>>();
                var userManager = app.ApplicationServices
                    .GetRequiredService<UserManager<UserEntity>>();
                AddTestUsers(roleManager, userManager).Wait();

                var context = app.ApplicationServices.GetRequiredService<UppsalaApiContext>();
                var dateLogicService = app.ApplicationServices.GetRequiredService<IDateLogicService>();
                AddTestData(context, dateLogicService, userManager);              
            }

            app.UseHsts(opt =>
            {
                opt.MaxAge(days: 180);
                opt.IncludeSubdomains();
                opt.Preload();
            });


            app.UseOAuthValidation();
            //app.UseOpenIddict();
            app.UseOpenIddictServer();

            app.UseResponseCaching(); // before MVC
            app.UseMvc();

            //TODO: this is not working i need to upgrade the package
            //app.UseApiVersioning();
           
        }


        private static async Task AddTestUsers(
            RoleManager<UserRoleEntity> roleManager,
            UserManager<UserEntity> userManager)
        {
            // Add a test role
            await roleManager.CreateAsync(new UserRoleEntity("Admin"));

            // Add a test user
            var user = new UserEntity
            {
                Email = "admin@landon.local",
                UserName = "admin@landon.local",
                FirstName = "Admin",
                LastName = "Testerman",
                CreatedAt = DateTimeOffset.UtcNow
            };

            await userManager.CreateAsync(user, "Supersecret123!!");

            // Put the user in the admin role
            await userManager.AddToRoleAsync(user, "Admin");
            await userManager.UpdateAsync(user);
        }


        private static void AddTestData(
            UppsalaApiContext context, 
            IDateLogicService dateLogicService,
            UserManager<UserEntity> userManager)
        {
            var roomA300 = context.Rooms.Add(new RoomEntity
            {
                Id = Guid.Parse("301df04d-8679-4b1b-ab92-0a586ae53d08"),
                Name = "Lecture Hall A300",
                Rate = 23959
            }).Entity;

            context.Rooms.Add(new RoomEntity
            {
                Id = Guid.Parse("ee2b83be-91db-4de5-8122-0a586ae59576"),
                Name = "Lecture Hall B200",
                Rate = 10119
            });


            var today = DateTimeOffset.Now;
            var start = dateLogicService.AlignStartTime(today);
            var end = start.Add(dateLogicService.GetMinimumStay());
            var adminUser = userManager.Users
                .SingleOrDefault(u => u.Email == "admin@landon.local");


            context.Bookings.Add(new BookingEntity
            {
                Id = Guid.Parse("2eac8dea-2749-42b3-9d21-8eb2fc0fd6bd"),
                Room = roomA300,
                CreatedAt = DateTimeOffset.UtcNow,
                StartAt = start,
                EndAt = end,
                Total = roomA300.Rate,
                User= adminUser
            });

            context.SaveChanges();
        }
    }
}
