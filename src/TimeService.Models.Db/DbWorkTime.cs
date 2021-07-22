using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace LT.DigitalOffice.TimeService.Models.Db
{
    public class DbWorkTime
    {
        public const string TableName = "WorkTimes";

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid CreatedBy { get; set; }
        public int Minutes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class DbWorkTimeConfiguration : IEntityTypeConfiguration<DbWorkTime>
    {
        public void Configure(EntityTypeBuilder<DbWorkTime> builder)
        {
            builder.
                ToTable(DbWorkTime.TableName);

            builder.
                HasKey(p => p.Id);

            builder
                .Property(p => p.Title)
                .IsRequired();
        }
    }
}
