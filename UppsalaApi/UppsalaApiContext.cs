using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using UppsalaApi.Models;

namespace UppsalaApi
{
    public class UppsalaApiContext : DbContext
    {
        public UppsalaApiContext(DbContextOptions options) 
            : base(options) {}

        public DbSet<RoomEntity> Rooms { get; set; }

    }
}
