using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef.Migrations
{
    [DbContext(typeof(TimeServiceDbContext))]
    [Migration("20210802143300_AddIsActiveColumns")]
    public class AddIsActiveColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: DbWorkTime.TableName,
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: DbLeaveTime.TableName,
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: DbLeaveTime.TableName);

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: DbWorkTime.TableName);
        }
    }
}
