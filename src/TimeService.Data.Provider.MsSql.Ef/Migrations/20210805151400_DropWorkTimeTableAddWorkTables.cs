using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef.Migrations
{
    [DbContext(typeof(TimeServiceDbContext))]
    [Migration("20210805151400_DropWorkTimeTableAddWorkTables")]
    class DropWorkTimeTableAddWorkTables : Migration
    {
        protected override void Up(MigrationBuilder builder)
        {
            builder.DropTable(
                name: "WorkTimes");

            builder.CreateTable(
                name: DbWorkTime.TableName,
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    ProjectId = table.Column<Guid>(nullable: false),
                    WorkTimeMonthLimitId = table.Column<Guid>(nullable: false),
                    Year = table.Column<int>(nullable: false),
                    Month = table.Column<int>(nullable: false),
                    UserHours = table.Column<float>(nullable: false),
                    ManagerHours = table.Column<float>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(nullable: true),
                    ModifiedBy = table.Column<Guid>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTimes", x => x.Id);
                });

            builder.CreateTable(
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
                    ModifiedAtUtc = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTimeDayJobs", x => x.Id);
                });

            builder.CreateTable(
                name: DbWorkTimeMonthLimit.TableName,
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Year = table.Column<int>(nullable: false),
                    Month = table.Column<int>(nullable: false),
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

        protected override void Down(MigrationBuilder builder)
        {
            builder.DropTable(
                name: "WorkTimes");

            builder.DropTable(
                name: "WorkTimeDayJobs");

            builder.DropTable(
                name: "WorkTimeMonthLimits");
        }
    }
}