using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.Http;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Db
{
  public class DbWorkTimeDayJobMapper : IDbWorkTimeDayJobMapper
  {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DbWorkTimeDayJobMapper(IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    }

    public DbWorkTimeDayJob Map(CreateWorkTimeDayJobRequest request)
    {
      if (request is null)
      {
        return null;
      }

      string cuttedDescription = request.Description?.Trim();

      return new DbWorkTimeDayJob
      {
        Id = Guid.NewGuid(),
        WorkTimeId = request.WorkTimeId,
        Day = request.Day,
        Description = string.IsNullOrEmpty(cuttedDescription) ? null : cuttedDescription,
        Name = request.Name?.Trim(),
        Minutes = request.Minutes,
        IsActive = true,
        CreatedAtUtc = DateTime.UtcNow,
        CreatedBy = _httpContextAccessor.HttpContext.GetUserId()
      };
    }
  }
}
