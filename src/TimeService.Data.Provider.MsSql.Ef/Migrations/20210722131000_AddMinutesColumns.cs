using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef.Migrations
{
    [DbContext(typeof(TimeServiceDbContext))]
    [Migration("20210722131000_AddMinutesColumns")]
    public class AddMinutesColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Minutes",
                table: DbWorkTime.TableName,
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "Minutes",
                table: DbLeaveTime.TableName,
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Minutes",
                table: DbWorkTime.TableName);

            migrationBuilder.DropColumn(
                name: "Minutes",
                table: DbLeaveTime.TableName);
        }
    }
}
