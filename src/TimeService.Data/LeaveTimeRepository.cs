using System;
using System.Collections.Generic;
using System.Linq;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Data
{
  public class LeaveTimeRepository : ILeaveTimeRepository
  {
    private readonly IDataProvider _provider;

    public LeaveTimeRepository(IDataProvider provider)
    {
      _provider = provider;
    }

    public Guid Add(DbLeaveTime dbLeaveTime)
    {
      _provider.LeaveTimes.Add(dbLeaveTime);
      _provider.Save();

      return dbLeaveTime.Id;
    }

    public bool Edit(DbLeaveTime leaveTime, JsonPatchDocument<DbLeaveTime> request)
    {
      if (leaveTime == null)
      {
        throw new ArgumentNullException(nameof(leaveTime));
      }

      request.ApplyTo(leaveTime);
      _provider.Save();

      return true;
    }

    public List<DbLeaveTime> Find(FindLeaveTimesFilter filter, out int totalCount)
    {
      if (filter.SkipCount < 0)
      {
        throw new BadRequestException("Skip count can't be less than 0.");
      }

      if (filter.TakeCount <= 0)
      {
        throw new BadRequestException("Take count can't be equal or less than 0.");
      }

      if (filter == null)
      {
        throw new ArgumentNullException(nameof(filter));
      }

      var dbLeaveTimes = _provider.LeaveTimes.AsQueryable();

      if (filter.UserId.HasValue)
      {
        dbLeaveTimes = dbLeaveTimes.Where(x => x.UserId == filter.UserId);
      }

      if (filter.StartTime.HasValue)
      {
        dbLeaveTimes = dbLeaveTimes.Where(x => x.StartTime >= filter.StartTime);
      }

      if (filter.EndTime.HasValue)
      {
        dbLeaveTimes = dbLeaveTimes.Where(x => x.EndTime <= filter.EndTime);
      }

      if (!(filter.IncludeDeactivated.HasValue && filter.IncludeDeactivated.Value))
      {
        dbLeaveTimes = dbLeaveTimes.Where(x => x.IsActive);
      }

      totalCount = dbLeaveTimes.Count();

      return dbLeaveTimes.Skip(filter.SkipCount).Take(filter.TakeCount).ToList();
    }

    public List<DbLeaveTime> Find(List<Guid> usersIds, int year, int month)
    {
      if (usersIds == null)
      {
        return null;
      }

      return _provider.LeaveTimes
        .Where(
          lt =>
            usersIds.Contains(lt.UserId)
            && (lt.StartTime.Month == month && lt.StartTime.Year == year
            || lt.EndTime.Month == month && lt.EndTime.Year == year)).ToList();
    }

    public DbLeaveTime Get(Guid leaveTimeId)
    {
      return _provider.LeaveTimes.FirstOrDefault(lt => lt.Id == leaveTimeId)
        ?? throw new NotFoundException($"No leave time with id {leaveTimeId}.");
    }
  }
}
