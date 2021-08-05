using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace LT.DigitalOffice.TimeService.Models.Db
{
    public class DbWorkTimeMonthLimit
    {
        public const string TableName = "WorkTimeMonthLimits";

        public Guid Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public float NormHours { get; set; }
        public string Holidays { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedAtUtc { get; set; }
    }

    public class DbDbWorkTimeMonthLimitConfiguration : IEntityTypeConfiguration<DbWorkTimeMonthLimit>
    {
        public void Configure(EntityTypeBuilder<DbWorkTimeMonthLimit> builder)
        {
            builder
                .ToTable(DbWorkTimeMonthLimit.TableName);

            builder
                .HasKey(k => k.Id);
        }
    }
}
