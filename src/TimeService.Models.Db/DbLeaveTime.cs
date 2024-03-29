﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace LT.DigitalOffice.TimeService.Models.Db
{
  public class DbLeaveTime
  {
    public const string TableName = "LeaveTimes";

    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public Guid UserId { get; set; }
    public int Minutes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int LeaveType { get; set; }
    public string Comment { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsClosed { get; set; }
    public bool IsActive { get; set; }

    public DbLeaveTime Parent { get; set; }
    public DbLeaveTime ManagerLeaveTime { get; set; }
  }

  public class DbLeaveTimeConfiguration : IEntityTypeConfiguration<DbLeaveTime>
  {
    public void Configure(EntityTypeBuilder<DbLeaveTime> builder)
    {
      builder
        .ToTable(DbLeaveTime.TableName);

      builder
        .HasKey(p => p.Id);

      builder
        .HasOne(lt => lt.Parent)
        .WithOne(lt => lt.ManagerLeaveTime);
    }
  }
}
