using System;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef.Migrations
{
  [DbContext(typeof(TimeServiceDbContext))]
  [Migration("20220118174900_CreateModuleSettingsTable")]
  public class AddModuleSettingsTable : Migration
  {
    private void CreateModuleSettingsTable(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: DbModuleSetting.TableName,
        columns: table => new
        {
          Id = table.Column<Guid>(nullable: false),
          WorkDaysApiUrl = table.Column<string>(nullable: false),
          CreatedAtUtc = table.Column<DateTime>(nullable: false),
          ModifiedBy = table.Column<Guid>(nullable: true),
          ModifiedAtUtc = table.Column<DateTime>(nullable: true)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_ModuleSettings", x => x.Id);
        });
    }
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      CreateModuleSettingsTable(migrationBuilder);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(DbModuleSetting.TableName);
    }
  }
}
