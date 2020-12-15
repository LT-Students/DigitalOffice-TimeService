using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace LT.DigitalOffice.TimeManagementService.Models.Db
{
    public class DbWorkTime
    {
        public const string TableName = "WorkTimes";

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Minutes { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid ProjectId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }

        public class DbProjectConfiguration : IEntityTypeConfiguration<DbWorkTime>
        {
            public void Configure(EntityTypeBuilder<DbWorkTime> builder)
            {
                builder
                    .ToTable(TableName);

                builder
                    .HasKey(p => p.Id);

                builder
                    .Property(p => p.UserId)
                    .IsRequired();

                builder
                    .Property(p => p.StartDate)
                    .IsRequired();

                builder
                    .Property(p => p.EndDate)
                    .IsRequired();

                builder
                    .Property(p => p.Minutes)
                    .IsRequired();

                builder
                    .Property(p => p.Title)
                    .IsRequired();

                builder
                    .Property(p => p.ProjectId)
                    .IsRequired();

                builder
                    .Property(p => p.CreatedAt)
                    .IsRequired();

                builder
                    .Property(p => p.CreatedBy)
                    .IsRequired();
            }
        }
    }
}
