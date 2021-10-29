using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LT.DigitalOffice.TimeService.Models.Db
{
  public class DbWorkTime
  {
    public const string TableName = "WorkTimes";

    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public Guid UserId { get; set; }
    public Guid ProjectId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public float? Hours { get; set; }
    public string Description { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public Guid? ModifiedBy { get; set; }

    public DbWorkTime Parent { get; set; }
    public DbWorkTime ManagerWorkTime { get; set; }
    public ICollection<DbWorkTimeDayJob> WorkTimeDayJobs { get; set; }

    public DbWorkTime()
    {
      WorkTimeDayJobs = new HashSet<DbWorkTimeDayJob>();
    }
  }

  public class DbWorkTimeConfiguration : IEntityTypeConfiguration<DbWorkTime>
  {
    public void Configure(EntityTypeBuilder<DbWorkTime> builder)
    {
      builder
        .ToTable(DbWorkTime.TableName);

      builder
        .HasKey(p => p.Id);

      builder
        .HasMany(d => d.WorkTimeDayJobs)
        .WithOne(w => w.WorkTime);

      builder
        .HasOne(t => t.Parent)
        .WithOne(t => t.ManagerWorkTime);
    }
  }
}
