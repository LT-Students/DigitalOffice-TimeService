using System;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef.Database.Migrations
{
    [DbContext(typeof(TimeServiceDbContext))]
    [Migration("20210518000000_InitialCreate")]
    public class InitialCreate : Migration
    {
        private const string ColumnIdName = "Id";

        #region Create tables

        private void CreateTableWorkTimes(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: DbWorkTime.TableName,
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    ProjectId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<Guid>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTimes", x => x.Id);
                });
        }

        private void CreateTableLeaveTimes(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: DbLeaveTime.TableName,
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<Guid>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    LeaveType = table.Column<int>(nullable: false),
                    Comment = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTimes", x => x.Id);
                });
        }

        #endregion

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CreateTableWorkTimes(migrationBuilder);

            CreateTableLeaveTimes(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(DbWorkTime.TableName);

            migrationBuilder.DropTable(DbLeaveTime.TableName);
        }
    }
}
