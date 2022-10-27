using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.TimeService.Data
{
  public class LeaveTimeRepository : ILeaveTimeRepository
  {
    private readonly IDataProvider _provider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private IQueryable<DbLeaveTime> CreateQueryable(FindLeaveTimesFilter filter)
    {
      IQueryable<DbLeaveTime> dbLeaveTimes = _provider.LeaveTimes.Where(lt => lt.ParentId == null).Include(x => x.ManagerLeaveTime).AsQueryable();

      if (filter.UserId.HasValue)
      {
        dbLeaveTimes = dbLeaveTimes.Where(x => x.UserId == filter.UserId);
      }

      if (filter.StartTime.HasValue)
      {
        dbLeaveTimes = dbLeaveTimes.Where(x =>
          x.ManagerLeaveTime == null && (x.StartTime >= filter.StartTime || x.EndTime > filter.StartTime)
          || x.ManagerLeaveTime != null && (x.ManagerLeaveTime.StartTime >= filter.StartTime || x.ManagerLeaveTime.EndTime > filter.StartTime));
      }

      if (filter.EndTime.HasValue)
      {
        dbLeaveTimes = dbLeaveTimes.Where(x =>
          x.ManagerLeaveTime == null && (x.EndTime <= filter.EndTime || x.StartTime < filter.EndTime)
          || x.ManagerLeaveTime != null && (x.ManagerLeaveTime.EndTime <= filter.EndTime || x.ManagerLeaveTime.StartTime < filter.EndTime));
      }

      if (!(filter.IncludeDeactivated.HasValue && filter.IncludeDeactivated.Value))
      {
        dbLeaveTimes = dbLeaveTimes.Where(x => x.IsActive);
      }

      return dbLeaveTimes;
    }

    public LeaveTimeRepository(
      IDataProvider provider,
      IHttpContextAccessor httpContextAccessor)
    {
      _provider = provider;
      _httpContextAccessor = httpContextAccessor;
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
      leaveTime.CreatedBy = _httpContextAccessor.HttpContext.GetUserId();
      leaveTime.CreatedAtUtc = DateTime.UtcNow;

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
        return Task.FromResult<List<DbLeaveTime>>(null);
      }

      IQueryable<DbLeaveTime> dbLeaveTimes = _provider.LeaveTimes.Where(lt => lt.ParentId == null && usersIds.Contains(lt.UserId))
        .Include(lt => lt.ManagerLeaveTime);

      if (isActive.HasValue)
      {
        dbLeaveTimes = dbLeaveTimes.Where(lt =>
          lt.ManagerLeaveTime == null && lt.IsActive == isActive.Value
          || lt.ManagerLeaveTime != null && lt.ManagerLeaveTime.IsActive == isActive.Value);
      }

      if (month is not null)
      {
        int countMonths = year * 12 + month.Value;

        dbLeaveTimes = dbLeaveTimes.Where(lt =>
          lt.ManagerLeaveTime == null
            && lt.StartTime.Month + lt.StartTime.Year * 12 <= countMonths
            && lt.EndTime.Month + lt.EndTime.Year * 12 >= countMonths
          || lt.ManagerLeaveTime != null
            && lt.ManagerLeaveTime.StartTime.Month + lt.ManagerLeaveTime.StartTime.Year * 12 <= countMonths
            && lt.ManagerLeaveTime.EndTime.Month + lt.ManagerLeaveTime.EndTime.Year * 12 >= countMonths);
      }
      else
      {
        dbLeaveTimes = dbLeaveTimes.Where(lt =>
          lt.ManagerLeaveTime == null && lt.StartTime.Year <= year && lt.EndTime.Year >= year
          || lt.ManagerLeaveTime != null && lt.ManagerLeaveTime.StartTime.Year <= year && lt.ManagerLeaveTime.EndTime.Year >= year);
      }

      return dbLeaveTimes.ToListAsync();
    }

    public Task<DbLeaveTime> GetAsync(Guid leaveTimeId)
    {
      return _provider.LeaveTimes.Include(lt => lt.ManagerLeaveTime).FirstOrDefaultAsync(lt => lt.Id == leaveTimeId);
    }

    public Task<bool> HasOverlapAsync(Guid userId, DateTime start, DateTime? end)
    {
      IQueryable<DbLeaveTime> leaveTimesQuery = _provider.LeaveTimes.Include(lt => lt.ManagerLeaveTime)
        .Where(lt => lt.UserId == userId && (lt.ManagerLeaveTime == null && lt.IsActive || lt.ManagerLeaveTime != null && lt.ManagerLeaveTime.IsActive));

      if (end.HasValue)
      {
        leaveTimesQuery = leaveTimesQuery.Where(lt =>
          lt.ManagerLeaveTime == null
            && (lt.LeaveType == (int)LeaveType.Prolonged && !lt.IsClosed && lt.StartTime <= end
              || start >= lt.StartTime && start <= lt.EndTime
              || end >= lt.StartTime && end <= lt.EndTime
              || start <= lt.StartTime && end >= lt.EndTime)
          || lt.ManagerLeaveTime != null
            && (lt.LeaveType == (int)LeaveType.Prolonged && !lt.IsClosed && lt.StartTime <= end
              || start >= lt.StartTime && start <= lt.EndTime
              || end >= lt.StartTime && end <= lt.EndTime
              || start <= lt.StartTime && end >= lt.EndTime));
      }
      else
      {
        leaveTimesQuery = leaveTimesQuery.Where(lt =>
          lt.ManagerLeaveTime == null
            && (lt.LeaveType == (int)LeaveType.Prolonged && !lt.IsClosed // if user already has prolonged leave time, can't add another
              || start <= lt.EndTime)
          || lt.ManagerLeaveTime != null
            && (lt.ManagerLeaveTime.LeaveType == (int)LeaveType.Prolonged && !lt.ManagerLeaveTime.IsClosed // if user already has manager prolonged leave time, can't add another
              || start <= lt.ManagerLeaveTime.EndTime));
      }

      return leaveTimesQuery.AnyAsync();
    }

    public Task<bool> HasOverlapAsync(DbLeaveTime leaveTime, DateTime start, DateTime end)
    {
      IQueryable<DbLeaveTime> leaveTimesQuery = _provider.LeaveTimes.Include(lt => lt.ManagerLeaveTime).Where(lt =>
        lt.UserId == leaveTime.UserId
        && (lt.ManagerLeaveTime == null && lt.IsActive && lt.Id != leaveTime.Id
          || lt.ManagerLeaveTime != null && lt.ManagerLeaveTime.IsActive && lt.Id != leaveTime.Id && lt.ManagerLeaveTime.Id != leaveTime.Id));

      return _provider.LeaveTimes.AnyAsync(lt =>
        lt.ManagerLeaveTime == null
          && (start >= lt.StartTime && start <= lt.EndTime
            || end >= lt.StartTime && end <= lt.EndTime
            || start <= lt.StartTime && end >= lt.EndTime)
        || lt.ManagerLeaveTime != null
          && (start >= lt.ManagerLeaveTime.StartTime && start <= lt.ManagerLeaveTime.EndTime
            || end >= lt.ManagerLeaveTime.StartTime && end <= lt.ManagerLeaveTime.EndTime
            || start <= lt.ManagerLeaveTime.StartTime && end >= lt.ManagerLeaveTime.EndTime));
    }
  }
}
