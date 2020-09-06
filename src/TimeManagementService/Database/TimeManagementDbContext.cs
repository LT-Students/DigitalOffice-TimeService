using LT.DigitalOffice.TimeManagementService.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LT.DigitalOffice.TimeManagementService.Database
{
    /// <summary>
    /// A class that defines the tables and its properties in the database of TimeManagementService.
    /// </summary>
    public class TimeManagementDbContext : DbContext
    {
        public DbSet<DbLeaveTime> LeaveTimes { get; set; }
        public DbSet<DbWorkTime> WorkTimes { get; set; }

        public TimeManagementDbContext(DbContextOptions<TimeManagementDbContext> options) : base(options)
        {
        }

        // Fluent API is written here.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}