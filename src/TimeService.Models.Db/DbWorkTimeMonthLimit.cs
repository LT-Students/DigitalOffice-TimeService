using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Models.Db
{
    public class DbWorkTimeMonthLimit
    {
        public const string TableName = "WorkTimeMonthLimit";

        public Guid Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public float NormHours { get; set; }
        public string Holidays { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedAtUtc { get; set; }

        public ICollection<DbWorkTime> WorkTimes {get; set;}

        public DbWorkTimeMonthLimit()
        {
            WorkTimes = new HashSet<DbWorkTime>();
        }
    }

    public class DbDbWorkTimeMonthLimitConfiguration : IEntityTypeConfiguration<DbWorkTimeMonthLimit>
    {
        public void Configure(EntityTypeBuilder<DbWorkTimeMonthLimit> builder)
        {
            builder
                .ToTable(DbWorkTimeMonthLimit.TableName);

            builder
                .HasKey(k => k.Id);

            builder
                .HasMany(w => w.WorkTimes)
                .WithOne(l => l.WorkTimeMonthLimit);
        }
    }
}
