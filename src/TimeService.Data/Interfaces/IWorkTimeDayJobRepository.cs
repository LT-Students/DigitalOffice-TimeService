using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.AspNetCore.JsonPatch;
using System;

namespace LT.DigitalOffice.TimeService.Data.Interfaces
{
    [AutoInject]
    public interface IWorkTimeDayJobRepository
    {
        DbWorkTimeDayJob Get(Guid workTimeDayJobId, bool includeWorkTime = false);

        bool Contains(Guid workTimeDayJobId);

        Guid Create(DbWorkTimeDayJob dayJob);

        bool Edit(Guid workTimeDayJobId, JsonPatchDocument<DbWorkTimeDayJob> request);
    }
}
