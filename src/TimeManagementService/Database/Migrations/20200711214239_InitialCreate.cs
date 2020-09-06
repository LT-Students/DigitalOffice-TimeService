using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LT.DigitalOffice.TimeManagementService.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Leaves",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    WorkerId = table.Column<Guid>(nullable: false),
                    StartLeave = table.Column<DateTime>(nullable: false),
                    EndLeave = table.Column<DateTime>(nullable: false),
                    TypeLeave = table.Column<int>(nullable: false),
                    Comment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaves", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkTimes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    WorkerId = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Time = table.Column<TimeSpan>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    ProjectId = table.Column<Guid>(nullable: false),
                    Comment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTimes", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Leaves");

            migrationBuilder.DropTable(
                name: "WorkTimes");
        }
    }
}
