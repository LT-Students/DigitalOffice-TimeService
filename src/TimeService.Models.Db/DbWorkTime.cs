using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Models.Db
{
    public class DbWorkTime
    {
        public const string TableName = "WorkTimes";

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProjectId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public float? UserHours { get; set; }
        public float? ManagerHours { get; set; }
        public string Description { get; set; }
        public DateTime? ModifiedAtUtc { get; set; }
        public Guid? ModifiedBy { get; set; }

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
        }
    }
}
