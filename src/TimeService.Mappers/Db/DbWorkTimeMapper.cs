using System;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;

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

    public DbWorkTime Map(CreateWorkTimeRequest request)
    {
      if (request is null)
      {
        return null;
      }

      DateTime timeNow = DateTime.UtcNow;

      return new DbWorkTime
      {
        Id = Guid.NewGuid(),
        UserId = request.UserId,
        ProjectId = default,
        Year = timeNow.Year,
        Month = timeNow.Month,
        Hours = request.Hours,
        Description = request.Description
      };
    }
  }
}
