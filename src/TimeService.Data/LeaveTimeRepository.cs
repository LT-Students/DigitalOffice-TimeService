using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.TimeService.Data
{
  public class LeaveTimeRepository : ILeaveTimeRepository
  {
    private readonly IDataProvider _provider;

    private IQueryable<DbLeaveTime> CreateQueryble(FindLeaveTimesFilter filter)
    {
      var dbLeaveTimes = _provider.LeaveTimes.AsQueryable();

      if (filter.UserId.HasValue)
      {
        dbLeaveTimes = dbLeaveTimes.Where(x => x.UserId == filter.UserId);
      }

      if (filter.StartTime.HasValue)
      {
        dbLeaveTimes = dbLeaveTimes.Where(x => x.StartTime >= filter.StartTime || x.EndTime > filter.StartTime);
      }

      if (filter.EndTime.HasValue)
      {
        dbLeaveTimes = dbLeaveTimes.Where(x => x.EndTime <= filter.EndTime || x.StartTime < filter.EndTime);
      }

      if (!(filter.IncludeDeactivated.HasValue && filter.IncludeDeactivated.Value))
      {
        dbLeaveTimes = dbLeaveTimes.Where(x => x.IsActive);
      }

      return dbLeaveTimes;
    }

    public LeaveTimeRepository(IDataProvider provider)
    {
      _provider = provider;
    }

    public async Task<Guid?> CreateAsync(DbLeaveTime dbLeaveTime)
    {
      if (dbLeaveTime == null)
      {
        return null;
      }

      _provider.LeaveTimes.Add(dbLeaveTime);
      await _provider.SaveAsync();

      return dbLeaveTime.Id;
    }

    public async Task<bool> EditAsync(DbLeaveTime leaveTime, JsonPatchDocument<DbLeaveTime> request)
    {
      if (leaveTime == null)
      {
        return false;
      }

      request.ApplyTo(leaveTime);
      await _provider.SaveAsync();

      return true;
    }

    public async Task<(List<DbLeaveTime>, int totalCount)> FindAsync(FindLeaveTimesFilter filter)
    {
      IQueryable<DbLeaveTime> dbLeaveTimes = CreateQueryble(filter);

      return (await dbLeaveTimes.Skip(filter.SkipCount).Take(filter.TakeCount).ToListAsync(), await dbLeaveTimes.CountAsync());
    }

    public async Task<List<DbLeaveTime>> GetAsync(List<Guid> usersIds, int year, int month)
    {
      if (usersIds == null)
      {
        return null;
      }

      int countMonths = year * 12 + month;

      return await _provider.LeaveTimes
        .Where(
          lt =>
            usersIds.Contains(lt.UserId)
            && lt.StartTime.Month + lt.StartTime.Year * 12 <= countMonths
            && lt.EndTime.Month + lt.EndTime.Year * 12 >= countMonths).ToListAsync();
    }

    public async Task<DbLeaveTime> GetAsync(Guid leaveTimeId)
    {
      return await _provider.LeaveTimes.FirstOrDefaultAsync(lt => lt.Id == leaveTimeId);
    }

    public async Task<bool> HasOverlapAsync(Guid userId, DateTime start, DateTime end)
    {
      return !await _provider.LeaveTimes.AllAsync(oldLeaveTime => !oldLeaveTime.IsActive
        || oldLeaveTime.UserId != userId
        || end <= oldLeaveTime.StartTime || oldLeaveTime.EndTime <= start);
    }

    public async Task<bool> HasOverlapAsync(DbLeaveTime leaveTime, DateTime? newStart, DateTime? newEnd)
    {
      if (!newStart.HasValue && !newEnd.HasValue)
      {
        return false;
      }

      DateTime start = newStart ?? leaveTime.StartTime;
      DateTime end = newEnd ?? leaveTime.EndTime;

      return !await _provider.LeaveTimes.AllAsync(oldLeaveTime => !oldLeaveTime.IsActive
        || oldLeaveTime.UserId != leaveTime.UserId
        || end <= oldLeaveTime.StartTime || oldLeaveTime.EndTime <= start);
    }
  }
}
