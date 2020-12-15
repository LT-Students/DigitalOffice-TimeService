using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace LT.DigitalOffice.TimeManagementService.Models.Db
{
    public class DbLeaveTime
    {
        public const string TableName = "LeaveTimes";

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int LeaveType { get; set; }
        public string Comment { get; set; }

        public class DbLeaveTimeConfiguration : IEntityTypeConfiguration<DbLeaveTime>
        {
            public void Configure(EntityTypeBuilder<DbLeaveTime> builder)
            {
                builder
                    .ToTable(TableName);

                builder
                    .HasKey(p => p.Id);

                builder
                    .Property(p => p.UserId)
                    .IsRequired();

                builder
                    .Property(p => p.StartTime)
                    .IsRequired();

                builder
                    .Property(p => p.EndTime)
                    .IsRequired();

                builder
                    .Property(p => p.LeaveType)
                    .IsRequired();
            }
        }
    }
}
