using System;
using System.Collections.Generic;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Data.Interfaces
{
  [AutoInject]
  public interface IWorkTimeMonthLimitRepository
  {
    Guid Add(DbWorkTimeMonthLimit workTimeMonthLimit);

    void AddRange(List<DbWorkTimeMonthLimit> workTimeMonthsLimits);

    DbWorkTimeMonthLimit Get(int year, int month);

    List<DbWorkTimeMonthLimit> GetRange(int startYear, int startMonth, int endYear, int endMonth);

    List<DbWorkTimeMonthLimit> Find(FindWorkTimeMonthLimitsFilter filter, int skipCount, int takeCount, out int totalCount);

    List<DbWorkTimeMonthLimit> Find(FindWorkTimeMonthLimitsFilter filter);

    bool Edit(Guid workTimeMonthLimitId, JsonPatchDocument<DbWorkTimeMonthLimit> request);
  }
}
