using System;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.TimeService.Mappers.Db
{
  public class DbWorkTimeMapper : IDbWorkTimeMapper
  {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DbWorkTimeMapper(
      IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    }

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
        UserId = _httpContextAccessor.HttpContext.GetUserId(),
        ProjectId = default,
        Year = request.Year,
        Month = request.Month,
        Hours = request.Hours,
        Description = request.Description
      };
    }
  }
}
