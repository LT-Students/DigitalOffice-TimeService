using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef.Migrations
{
    [DbContext(typeof(TimeServiceDbContext))]
    [Migration("20210809012600_AddDayColumnToWorkTimeTable")]
    public class AddDayColumnToWorkTimeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Day",
                table: DbWorkTime.TableName,
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Day",
                table: DbWorkTime.TableName);
        }
    }
}
