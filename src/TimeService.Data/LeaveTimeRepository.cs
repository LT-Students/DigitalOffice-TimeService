using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.TimeService.Data
{
  public class LeaveTimeRepository : ILeaveTimeRepository
  {
    private readonly IDataProvider _provider;

    private IQueryable<DbLeaveTime> CreateQueryable(FindLeaveTimesFilter filter)
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
      IQueryable<DbLeaveTime> dbLeaveTimes = CreateQueryable(filter);

      return (await dbLeaveTimes.Skip(filter.SkipCount).Take(filter.TakeCount).ToListAsync(), await dbLeaveTimes.CountAsync());
    }

    public Task<List<DbLeaveTime>> GetAsync(List<Guid> usersIds, int year, int? month, bool? isActive = null)
    {
      if (usersIds == null)
      {
        return null;
      }

      IQueryable<DbLeaveTime> dbLeaveTimes = _provider.LeaveTimes.Where(lt => usersIds.Contains(lt.UserId));

      if (isActive.HasValue)
      {
        dbLeaveTimes = dbLeaveTimes.Where(lt => lt.IsActive == isActive.Value);
      }

      if (month is not null)
      {
        int countMonths = year * 12 + month.Value;

        dbLeaveTimes = dbLeaveTimes.Where(lt =>
          usersIds.Contains(lt.UserId)
          && lt.StartTime.Month + lt.StartTime.Year * 12 <= countMonths
          && lt.EndTime.Month + lt.EndTime.Year * 12 >= countMonths);
      }
      else
      {
        dbLeaveTimes = dbLeaveTimes.Where(lt =>
          lt.StartTime.Year <= year
          && lt.EndTime.Year >= year);
      }

      return dbLeaveTimes.ToListAsync();
    }

    public Task<DbLeaveTime> GetAsync(Guid leaveTimeId)
    {
      return _provider.LeaveTimes.FirstOrDefaultAsync(lt => lt.Id == leaveTimeId);
    }

    public Task<bool> HasOverlapAsync(Guid userId, DateTime start, DateTime? end)
    {
      return end.HasValue
        ? _provider.LeaveTimes.AnyAsync(dbLeaveTime =>
            dbLeaveTime.IsActive && dbLeaveTime.UserId == userId
            && (dbLeaveTime.LeaveType == (int)LeaveType.Prolonged && !dbLeaveTime.IsClosed
             || start >= dbLeaveTime.StartTime && start <= dbLeaveTime.EndTime
             || end >= dbLeaveTime.StartTime && end <= dbLeaveTime.EndTime
             || start <= dbLeaveTime.StartTime && end >= dbLeaveTime.EndTime))
        : _provider.LeaveTimes.AnyAsync(dbLeaveTime =>
            dbLeaveTime.IsActive && dbLeaveTime.UserId == userId
            && (dbLeaveTime.LeaveType == (int)LeaveType.Prolonged && !dbLeaveTime.IsClosed
              || start <= dbLeaveTime.EndTime));
    }

    public Task<bool> HasOverlapAsync(DbLeaveTime leaveTime, DateTime start, DateTime end)
    {
      return _provider.LeaveTimes.AnyAsync(dbLeaveTime =>
        dbLeaveTime.IsActive
        && dbLeaveTime.UserId == leaveTime.UserId
        && dbLeaveTime.Id != leaveTime.Id
        &&
        ((start >= dbLeaveTime.StartTime && start <= dbLeaveTime.EndTime)
        || (end >= dbLeaveTime.StartTime && end <= dbLeaveTime.EndTime)
        || (start <= dbLeaveTime.StartTime && end >= dbLeaveTime.EndTime)));
    }
  }
}
