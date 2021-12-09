using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.TimeService.Data
{
  public class WorkTimeRepository : IWorkTimeRepository
  {
    private readonly IDataProvider _provider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WorkTimeRepository(
      IDataProvider provider,
      IHttpContextAccessor httpContextAccessor)
    {
      _provider = provider;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Guid?> CreateAsync(DbWorkTime dbWorkTime)
    {
      if (dbWorkTime == null)
      {
        return null;
      }

      _provider.WorkTimes.Add(dbWorkTime);
      await _provider.SaveAsync();

      return dbWorkTime.Id;
    }

    public async Task<DbWorkTime> GetAsync(Guid id)
    {
      return await _provider.WorkTimes
        .Include(w => w.ManagerWorkTime)
        .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<(List<DbWorkTime>, int totalCount)> FindAsync(FindWorkTimesFilter filter)
    {
      var dbWorkTimes = _provider.WorkTimes.Include(wt => wt.ManagerWorkTime).Where(wt => !wt.ParentId.HasValue).AsQueryable();

      if (filter.UserId.HasValue)
      {
        dbWorkTimes = dbWorkTimes.Where(x => x.UserId == filter.UserId.Value);
      }

      if (filter.ProjectId.HasValue)
      {
        dbWorkTimes = dbWorkTimes.Where(x => x.ProjectId == filter.ProjectId.Value);
      }

      if (filter.IncludeDayJobs.HasValue && filter.IncludeDayJobs.Value)
      {
        dbWorkTimes = dbWorkTimes.Include(wt => wt.WorkTimeDayJobs.Where(dj => dj.IsActive));
      }

      if (filter.Month.HasValue)
      {
        dbWorkTimes = dbWorkTimes.Where(x => x.Month == filter.Month.Value);
      }

      if (filter.Year.HasValue)
      {
        dbWorkTimes = dbWorkTimes.Where(x => x.Year == filter.Year.Value);
      }

      return (await dbWorkTimes.Skip(filter.SkipCount).Take(filter.TakeCount).ToListAsync(), await dbWorkTimes.CountAsync());
    }

    public async Task<bool> EditAsync(DbWorkTime dbWorkTime, JsonPatchDocument<DbWorkTime> jsonPatchDocument)
    {
      if (dbWorkTime == null || jsonPatchDocument == null)
      {
        return false;
      }

      jsonPatchDocument.ApplyTo(dbWorkTime);
      dbWorkTime.ModifiedAtUtc = DateTime.UtcNow;
      dbWorkTime.ModifiedBy = _httpContextAccessor.HttpContext.GetUserId();
      await _provider.SaveAsync();

      return true;
    }

    public async Task<DbWorkTime> GetLastAsync()
    {
      return await _provider.WorkTimes
        .OrderByDescending(wt => wt.Year)
        .ThenByDescending(wt => wt.Month)
        .FirstOrDefaultAsync();
    }

    public async Task<bool> DoesExistAsync(Guid id)
    {
      return await _provider.WorkTimes.AnyAsync(wt => wt.Id == id);
    }

    public async Task<bool> DoesEmptyWorkTimeExistAsync(Guid userId, DateTime dateTime)
    {
      return await _provider.WorkTimes.AnyAsync(wt => wt.UserId == userId && wt.ProjectId == default
        && wt.Month == dateTime.Month && wt.Year == dateTime.Year);
    }

    public async Task<List<DbWorkTime>> GetAsync(List<Guid> usersIds, List<Guid> projectsIds, int year, int month, bool includeJobs = false)
    {
      if (usersIds == null)
      {
        return null;
      }

      IQueryable<DbWorkTime> workTimes = _provider.WorkTimes
        .Include(wt => wt.ManagerWorkTime)
        .Where(wt => !wt.ParentId.HasValue)
        .Where(wt =>
          wt.Year == year
          && wt.Month == month);

      if (usersIds != null)
      {
        workTimes = workTimes.Where(wt => usersIds.Contains(wt.UserId));
      }

      if (projectsIds != null)
      {
        workTimes = workTimes.Where(wt => projectsIds.Contains(wt.ProjectId));
      }

      if (includeJobs)
      {
        workTimes = workTimes.Include(wt => wt.WorkTimeDayJobs);
      }

      return await workTimes.ToListAsync();
    }
  }
}
