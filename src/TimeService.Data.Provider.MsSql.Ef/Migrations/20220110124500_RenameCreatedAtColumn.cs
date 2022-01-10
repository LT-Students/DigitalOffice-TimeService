using System;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef.Migrations
{
  [DbContext(typeof(TimeServiceDbContext))]
  [Migration("20220110124500_RenameCreatedAtColumn")]
  public class RenameCreatedAtColumn : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.RenameColumn(
        name: "CreatedAt",
        newName: "CreatedAtUtc",
        table: DbLeaveTime.TableName);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.RenameColumn(
        name: "CreatedAtUtc",
        newName: "CreatedAt",
        table: DbLeaveTime.TableName);
    }
  }
}
