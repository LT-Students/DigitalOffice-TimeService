using System;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.TimeService.Mappers.Db
{
  public class DbLeaveTimeMapper : IDbLeaveTimeMapper
  {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DbLeaveTimeMapper(IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    }

    public DbLeaveTime Map(CreateLeaveTimeRequest request)
    {
      if (request == null)
      {
        return null;
      }

      return new DbLeaveTime
      {
        Id = Guid.NewGuid(),
        UserId = request.UserId,
        CreatedBy = _httpContextAccessor.HttpContext.GetUserId(),
        LeaveType = (int)request.LeaveType,
        Minutes = request.Minutes,
        StartTime = request.StartTime,
        EndTime = request.EndTime,
        CreatedAt = DateTime.Now,
        Comment = !string.IsNullOrEmpty(request.Comment?.Trim()) ? request.Comment.Trim() : null,
        IsActive = true
      };
    }
  }
}
