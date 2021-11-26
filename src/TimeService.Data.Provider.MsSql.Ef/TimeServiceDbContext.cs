using System.Reflection;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Database;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef
{
  /// <summary>
  /// A class that defines the tables and its properties in the database of TimeService.
  /// </summary>
  public class TimeServiceDbContext : DbContext, IDataProvider
  {
    public DbSet<DbLeaveTime> LeaveTimes { get; set; }
    public DbSet<DbWorkTime> WorkTimes { get; set; }
    public DbSet<DbWorkTimeDayJob> WorkTimeDayJobs { get; set; }
    public DbSet<DbWorkTimeMonthLimit> WorkTimeMonthLimits { get; set; }

    public TimeServiceDbContext(DbContextOptions<TimeServiceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("LT.DigitalOffice.TimeService.Models.Db"));
    }

    void IBaseDataProvider.Save()
    {
      SaveChanges();
    }

    async Task IBaseDataProvider.SaveAsync()
    {
      await SaveChangesAsync();
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
