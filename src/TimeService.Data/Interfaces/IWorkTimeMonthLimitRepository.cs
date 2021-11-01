using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Data.Interfaces
{
  [AutoInject]
  public interface IWorkTimeMonthLimitRepository
  {
    Task<Guid?> CreateAsync(DbWorkTimeMonthLimit workTimeMonthLimit);

    Task CreateRangeAsync(List<DbWorkTimeMonthLimit> workTimeMonthsLimits);

    Task<DbWorkTimeMonthLimit> GetAsync(int year, int month);

    Task<List<DbWorkTimeMonthLimit>> GetAsync(int startYear, int startMonth, int endYear, int endMonth);

    Task<DbWorkTimeMonthLimit> GetLastAsync();

    Task<(List<DbWorkTimeMonthLimit> dbWorkTimeMonthLimit, int totalCount)> FindAsync(FindWorkTimeMonthLimitsFilter filter);

    Task<bool> EditAsync(Guid workTimeMonthLimitId, JsonPatchDocument<DbWorkTimeMonthLimit> request);
  }
}
