using System;
using LT.DigitalOffice.TimeManagementService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LT.DigitalOffice.ProjectService.Data.Provider.MsSql.Ef.Database.Migrations
{
    [DbContext(typeof(TimeManagementDbContext))]
    [Migration("20201210000000_InitialCreate")]
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
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    Minutes = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    ProjectId = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<Guid>(nullable: false)
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
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
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

        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
        }
    }
}
