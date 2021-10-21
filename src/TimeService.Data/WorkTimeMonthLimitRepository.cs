using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;

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

    public Guid Add(DbWorkTimeMonthLimit workTimeMonthLimit)
    {
      if (workTimeMonthLimit == null)
      {
        throw new ArgumentNullException(nameof(workTimeMonthLimit));
      }

      _provider.WorkTimeMonthLimits.Add(workTimeMonthLimit);
      _provider.Save();

      return workTimeMonthLimit.Id;
    }

    public void AddRange(List<DbWorkTimeMonthLimit> workTimeMonthsLimits)
    {
      if (workTimeMonthsLimits == null || workTimeMonthsLimits.Contains(null))
      {
        throw new ArgumentNullException(nameof(workTimeMonthsLimits));
      }

      _provider.WorkTimeMonthLimits.AddRange(workTimeMonthsLimits);
      _provider.Save();
    }

    public bool Edit(Guid workTimeMonthLimitId, JsonPatchDocument<DbWorkTimeMonthLimit> request)
    {
      DbWorkTimeMonthLimit dbWorkTimeMonthLimit = _provider.WorkTimeMonthLimits.FirstOrDefault(ml => ml.Id == workTimeMonthLimitId)
          ?? throw new NotFoundException($"No worktime month limits with id {workTimeMonthLimitId}");

      request.ApplyTo(dbWorkTimeMonthLimit);
      dbWorkTimeMonthLimit.ModifiedAtUtc = DateTime.UtcNow;
      dbWorkTimeMonthLimit.ModifiedBy = _httpContextAccessor.HttpContext.GetUserId();
      _provider.Save();

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

    /*public async Task<List<DbWorkTimeMonthLimit>> FindAllAsync(FindWorkTimeMonthLimitsFilter filter)
    {
      if (filter == null)
      {
        throw new ArgumentNullException(nameof(filter));
      }

      var dbWorkTimeMonthLimits = _provider.WorkTimeMonthLimits.AsQueryable();

      if (filter.Month.HasValue)
      {
        dbWorkTimeMonthLimits = dbWorkTimeMonthLimits.Where(x => x.Month == filter.Month.Value);
      }

      if (filter.Year.HasValue)
      {
        dbWorkTimeMonthLimits = dbWorkTimeMonthLimits.Where(x => x.Year == filter.Year.Value);
      }

      return dbWorkTimeMonthLimits.ToList();
    }*/

    public DbWorkTimeMonthLimit Get(int year, int month)
    {
      return _provider.WorkTimeMonthLimits.FirstOrDefault(l => l.Year == year && l.Month == month);
    }

    public List<DbWorkTimeMonthLimit> GetRange(int startYear, int startMonth, int endYear, int endMonth)
    {
      int startCountMonths = startYear * 12 + startMonth;
      int endCountMonths = endYear * 12 + endMonth;

      if (endCountMonths < startCountMonths)
      {
        return null;
      }

      return _provider.WorkTimeMonthLimits
        .Where(ml =>
          ml.Year * 12 + ml.Month >= startCountMonths && ml.Year * 12 + ml.Month <= endCountMonths).ToList();
    }
  }
}
