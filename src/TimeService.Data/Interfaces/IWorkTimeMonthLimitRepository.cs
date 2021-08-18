using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Data.Interfaces
{
    [AutoInject]
    public interface IWorkTimeMonthLimitRepository
    {
        Guid Add(DbWorkTimeMonthLimit workTimeMonthLimit);

        DbWorkTimeMonthLimit Get(int year, int month);

        DbWorkTimeMonthLimit GetLast();

        List<DbWorkTimeMonthLimit> Find(FindWorkTimeMonthLimitsFilter filter, int skipCount, int takeCount, out int totalCount);

        List<DbWorkTimeMonthLimit> Find(FindWorkTimeMonthLimitsFilter filter);

        bool Edit(Guid workTimeMonthLimitId, JsonPatchDocument<DbWorkTimeMonthLimit> request);
    }
}
