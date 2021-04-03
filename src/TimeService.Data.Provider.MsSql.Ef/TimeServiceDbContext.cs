using LT.DigitalOffice.Kernel.Database;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef
{
    /// <summary>
    /// A class that defines the tables and its properties in the database of TimeService.
    /// </summary>
    public class TimeServiceDbContext : DbContext, IDataProvider
    {
        public DbSet<DbLeaveTime> LeaveTimes { get; set; }
        public DbSet<DbWorkTime> WorkTimes { get; set; }

        public TimeServiceDbContext(DbContextOptions<TimeServiceDbContext> options) : base(options)
        {
        }

        // Fluent API is written here.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("LT.DigitalOffice.TimeService.Models.Db"));
        }

        void IBaseDataProvider.Save()
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