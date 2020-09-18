using LT.DigitalOffice.TimeManagementService.Models.Db;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LT.DigitalOffice.TimeManagementService.Data.Provider.MsSql.Ef
{
    /// <summary>
    /// A class that defines the tables and its properties in the database of TimeManagementService.
    /// </summary>
    public class TimeManagementDbContext : DbContext, IDataProvider
    {
        public DbSet<DbLeaveTime> LeaveTimes { get; set; }
        public DbSet<DbWorkTime> WorkTimes { get; set; }

        public TimeManagementDbContext(DbContextOptions<TimeManagementDbContext> options) : base(options)
        {
        }

        // Fluent API is written here.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("LT.DigitalOffice.TimeManagementService.Models.Db"));
        }

        void IDataProvider.SaveChanges()
        {
            SaveChanges();
        }

        public object MakeEntityDetached(object obj)
        {
            Entry(obj).State = EntityState.Detached;

            return Entry(obj).State;
        }

        public void EnsureDeleted()
        {
            Database.EnsureDeleted();
        }

        public bool IsInMemory()
        {
            return Database.IsInMemory();
        }
    }
}