using System;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef.Migrations
{
  [DbContext(typeof(TimeServiceDbContext))]
  [Migration("20211029110300_UpdateWorkTimesTable")]
  public class UpdateWorkTimesTable : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
        name: "ManagerHours",
        table: DbWorkTime.TableName);

      migrationBuilder.RenameColumn(
        name: "UserHours",
        newName: "Hours",
        table: DbWorkTime.TableName);

      migrationBuilder.AddColumn<Guid?>(
        name: "ParentId",
        table: DbWorkTime.TableName,
        nullable: true);
    }
  }
}
