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
  public class WorkTimeMonthLimitRepository : IWorkTimeMonthLimitRepository
  {
    private readonly IDataProvider _provider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WorkTimeMonthLimitRepository(
      IDataProvider provider,
      IHttpContextAccessor httpContextAccessor)
    {
      _provider = provider;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Guid?> CreateAsync(DbWorkTimeMonthLimit workTimeMonthLimit)
    {
      if (workTimeMonthLimit == null)
      {
        return null;
      }

      _provider.WorkTimeMonthLimits.Add(workTimeMonthLimit);
      await _provider.SaveAsync();

      return workTimeMonthLimit.Id;
    }

    public async Task CreateRangeAsync(List<DbWorkTimeMonthLimit> workTimeMonthsLimits)
    {
      if (workTimeMonthsLimits == null || workTimeMonthsLimits.Contains(null))
      {
        return;
      }

      _provider.WorkTimeMonthLimits.AddRange(workTimeMonthsLimits);
      await _provider.SaveAsync();
    }

    public async Task<bool> EditAsync(Guid workTimeMonthLimitId, JsonPatchDocument<DbWorkTimeMonthLimit> request)
    {
      DbWorkTimeMonthLimit dbWorkTimeMonthLimit = _provider.WorkTimeMonthLimits.FirstOrDefault(ml => ml.Id == workTimeMonthLimitId);

      if (dbWorkTimeMonthLimit == null)
      {
        return false;
      }

      request.ApplyTo(dbWorkTimeMonthLimit);
      dbWorkTimeMonthLimit.ModifiedAtUtc = DateTime.UtcNow;
      dbWorkTimeMonthLimit.ModifiedBy = _httpContextAccessor.HttpContext.GetUserId();
      await _provider.SaveAsync();

      return true;
    }

    public async Task<(List<DbWorkTimeMonthLimit> dbWorkTimeMonthLimit, int totalCount)> FindAsync(FindWorkTimeMonthLimitsFilter filter)
    {
      if (filter == null)
      {
        return (null, default);
      }
      IQueryable<DbWorkTimeMonthLimit> dbWorkTimeMonthLimits = _provider.WorkTimeMonthLimits.AsQueryable();

      if (filter.Month.HasValue)
      {
        dbWorkTimeMonthLimits = dbWorkTimeMonthLimits.Where(x => x.Month == filter.Month.Value);
      }

      if (filter.Year.HasValue)
      {
        dbWorkTimeMonthLimits = dbWorkTimeMonthLimits.Where(x => x.Year == filter.Year.Value);
      }

      int totalCount = await dbWorkTimeMonthLimits.CountAsync();

      if (filter.TakeCount != default)
      {
        dbWorkTimeMonthLimits = dbWorkTimeMonthLimits.Skip(filter.SkipCount).Take(filter.TakeCount);
      }

      return (await dbWorkTimeMonthLimits.ToListAsync(), totalCount);
    }

    public async Task<DbWorkTimeMonthLimit> GetAsync(int year, int month)
    {
      return await _provider.WorkTimeMonthLimits.FirstOrDefaultAsync(l => l.Year == year && l.Month == month);
    }

    public async Task<DbWorkTimeMonthLimit> GetLastAsync()
    {
      return await _provider.WorkTimeMonthLimits.OrderByDescending(l => l.Year).ThenByDescending(l => l.Month).FirstOrDefaultAsync();
    }

    public async Task<List<DbWorkTimeMonthLimit>> GetAsync(int startYear, int startMonth, int endYear, int endMonth)
    {
      int startCountMonths = startYear * 12 + startMonth;
      int endCountMonths = endYear * 12 + endMonth;

      if (endCountMonths < startCountMonths)
      {
        return null;
      }

      return await _provider.WorkTimeMonthLimits
        .Where(ml => ml.Year * 12 + ml.Month >= startCountMonths && ml.Year * 12 + ml.Month <= endCountMonths)
        .ToListAsync();
    }
  }
}
