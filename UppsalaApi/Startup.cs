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
            services.AddDbContext<UppsalaApiContext>(opt => opt.UseInMemoryDatabase());

            services.AddAutoMapper();

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
            //services.AddScoped<IUserService, DefaultUserService>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Add some test data in development
            if(env.IsDevelopment())
            {
                var context = app.ApplicationServices.GetRequiredService<UppsalaApiContext>();
                var dateLogicService = app.ApplicationServices.GetRequiredService<IDateLogicService>();
                AddTestData(context, dateLogicService);              
            }


            app.UseHsts(opt =>
            {
                opt.MaxAge(days: 180);
                opt.IncludeSubdomains();
                opt.Preload();
            });
            app.UseMvc();

            //TODO: this is not working i need to upgrade the package
            //app.UseApiVersioning();
           
        }


        private static void AddTestData(UppsalaApiContext context, IDateLogicService dateLogicService)
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

            context.Bookings.Add(new BookingEntity
            {
                Id = Guid.Parse("2eac8dea-2749-42b3-9d21-8eb2fc0fd6bd"),
                Room = roomA300,
                CreatedAt = DateTimeOffset.UtcNow,
                StartAt = start,
                EndAt = end,
                Total = roomA300.Rate,
            });


            context.SaveChanges();
        }
    }
}
