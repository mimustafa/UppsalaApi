using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UppsalaApi.Models;

namespace UppsalaApi
{
    public class UppsalaApiContext : IdentityDbContext<UserEntity, UserRoleEntity, Guid>
    {
        public UppsalaApiContext(DbContextOptions options) 
            : base(options) {}

        public DbSet<RoomEntity> Rooms { get; set; }

        public DbSet<BookingEntity> Bookings { get; set; }

    }
}
