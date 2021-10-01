using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace LT.DigitalOffice.TimeService.Models.Db
{
    public class DbWorkTimeDayJob
    {
        public static string TableName = "WorkTimeDayJobs";

        public Guid Id { get; set; }
        public Guid WorkTimeId { get; set; }
        public int Day { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Minutes { get; set; }
        public bool IsActive { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedAtUtc { get; set; }

        public DbWorkTime WorkTime { get; set; }
    }

    public class DbWorkTimeDayJobsConfiguration : IEntityTypeConfiguration<DbWorkTimeDayJob>
    {
        public void Configure(EntityTypeBuilder<DbWorkTimeDayJob> builder)
        {
            builder
                .ToTable(DbWorkTimeDayJob.TableName);

            builder
                .HasKey(k => k.Id);
        }
    }
}