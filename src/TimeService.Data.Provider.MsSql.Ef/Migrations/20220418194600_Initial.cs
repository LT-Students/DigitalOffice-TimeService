using System;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef.Migrations
{
  [DbContext(typeof(TimeServiceDbContext))]
  [Migration("20220418194600_Initial")]
  public class Initial : Migration
  {
    #region private methods
    private void CreateWorkTimesTable(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: DbWorkTime.TableName,
        columns: table => new
        {
          Id = table.Column<Guid>(nullable: false),
          ParentId = table.Column<Guid>(nullable: true),
          UserId = table.Column<Guid>(nullable: false),
          ProjectId = table.Column<Guid>(nullable: false),
          Year = table.Column<int>(nullable: false),
          Month = table.Column<int>(nullable: false),
          Hours = table.Column<float>(nullable: true),
          Description = table.Column<string>(nullable: true),
          ModifiedAtUtc = table.Column<DateTime>(nullable: true),
          ModifiedBy = table.Column<Guid>(nullable: true)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_WorkTimes", x => x.Id);
        });
    }

    private void CreateLeaveTimesTable(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: DbLeaveTime.TableName,
        columns: table => new
        {
          Id = table.Column<Guid>(nullable: false),
          UserId = table.Column<Guid>(nullable: false),
          StartTime = table.Column<DateTime>(nullable: false),
          EndTime = table.Column<DateTime>(nullable: false),
          Minutes = table.Column<int>(nullable: false),
          LeaveType = table.Column<int>(nullable: false),
          Comment = table.Column<string>(nullable: true),
          CreatedBy = table.Column<Guid>(nullable: false),
          CreatedAtUtc = table.Column<DateTime>(nullable: false),
          IsActive = table.Column<bool>(nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_LeaveTimes", x => x.Id);
        });
    }

    private void CreateWorkTimeDayJobsTable(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: DbWorkTimeDayJob.TableName,
        columns: table => new
        {
          Id = table.Column<Guid>(nullable: false),
          WorkTimeId = table.Column<Guid>(nullable: false),
          Day = table.Column<int>(nullable: false),
          Name = table.Column<string>(nullable: false),
          Description = table.Column<string>(nullable: true),
          Minutes = table.Column<int>(nullable: false),
          CreatedBy = table.Column<Guid>(nullable: false),
          CreatedAtUtc = table.Column<DateTime>(nullable: false),
          ModifiedBy = table.Column<Guid>(nullable: true),
          ModifiedAtUtc = table.Column<DateTime>(nullable: true),
          IsActive = table.Column<bool>(nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_WorkTimeDayJobs", x => x.Id);
        });
    }

    private void CreateWorkTimeMonthLimitsTable(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: DbWorkTimeMonthLimit.TableName,
        columns: table => new
        {
          Id = table.Column<Guid>(nullable: false),
          Month = table.Column<int>(nullable: false),
          Year = table.Column<int>(nullable: false),
          NormHours = table.Column<float>(nullable: false),
          Holidays = table.Column<string>(nullable: false),
          ModifiedBy = table.Column<Guid>(nullable: true),
          ModifiedAtUtc = table.Column<DateTime>(nullable: true)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_WorkTimeMonthLimits", x => x.Id);
        });
    }
    #endregion

    protected override void Up(MigrationBuilder migrationBuilder)
    {
      CreateWorkTimesTable(migrationBuilder);

      CreateLeaveTimesTable(migrationBuilder);

      CreateWorkTimeDayJobsTable(migrationBuilder);

      CreateWorkTimeMonthLimitsTable(migrationBuilder);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(DbWorkTime.TableName);

      migrationBuilder.DropTable(DbLeaveTime.TableName);

      migrationBuilder.DropTable(DbWorkTimeDayJob.TableName);

      migrationBuilder.DropTable(DbWorkTimeMonthLimit.TableName);
    }
  }
}
