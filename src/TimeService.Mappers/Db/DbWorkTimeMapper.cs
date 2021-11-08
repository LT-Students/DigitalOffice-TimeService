using System;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;

namespace LT.DigitalOffice.TimeService.Mappers.Db
{
  public class DbWorkTimeMapper : IDbWorkTimeMapper
  {
    public DbWorkTime Map(DbWorkTime parent, Guid managerId)
    {
      if (parent == null)
      {
        return null;
      }

      return new DbWorkTime
      {
        Id = Guid.NewGuid(),
        Description = parent.Description,
        Hours = parent.Hours,
        Year = parent.Year,
        Month = parent.Month,
        UserId = parent.UserId,
        ParentId = parent.Id,
        ProjectId = parent.ProjectId,
        ModifiedAtUtc = DateTime.UtcNow,
        ModifiedBy = managerId
      };
    }
  }
}
