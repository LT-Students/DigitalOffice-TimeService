using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LT.DigitalOffice.TimeService.Models.Db
{
  public class DbModuleSetting
  {
    public static string TableName = "ModuleSettings";

    public Guid Id { get; set; }
    public string WorkDaysApiUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
  }

  public class DbModuleSettingsConfiguration : IEntityTypeConfiguration<DbModuleSetting>
  {
    public void Configure(EntityTypeBuilder<DbModuleSetting> builder)
    {
      builder
        .ToTable(DbModuleSetting.TableName);

      builder
        .HasKey(k => k.Id);
    }
  }
}
