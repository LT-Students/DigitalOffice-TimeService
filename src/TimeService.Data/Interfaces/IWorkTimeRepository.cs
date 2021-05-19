using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Data.Interfaces
{
    /// <summary>
    /// Represents interface of repository in repository pattern.
    /// Provides methods for working with the database of TimeService.
    /// </summary>
    [AutoInject]
    public interface IWorkTimeRepository
    {
        Guid Create(DbWorkTime workTime);

        DbWorkTime Get(Guid id);

        List<DbWorkTime> Find(FindWorkTimesFilter filter, int skipCount, int takeCount, out int totalCount);

        bool Edit(DbWorkTime dbWorkTime, JsonPatchDocument<DbWorkTime> jsonPatchDocument);
    }
}
