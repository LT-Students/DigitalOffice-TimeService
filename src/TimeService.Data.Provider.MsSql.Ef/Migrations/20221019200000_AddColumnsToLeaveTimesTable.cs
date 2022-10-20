using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef.Migrations
{
  [DbContext(typeof(TimeServiceDbContext))]
  [Migration("20221019200000_AddColumnsToLeaveTimesTable")]
  public class AddColumnsToLeaveTimesTable : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<bool>(
        name: "IsClosed",
        table: DbLeaveTime.TableName,
        defaultValue: true,
        nullable: false);

      migrationBuilder.AddColumn<Guid?>(
        name: "ParentId",
        table: DbLeaveTime.TableName,
        nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
        name: "IsClosed",
        table: DbLeaveTime.TableName);

      migrationBuilder.DropColumn(
        name: "ParentId",
        table: DbLeaveTime.TableName);
    }
  }
}
