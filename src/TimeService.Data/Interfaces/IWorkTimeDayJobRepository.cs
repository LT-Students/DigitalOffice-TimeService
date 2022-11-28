using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Data.Interfaces
{
  [AutoInject]
  public interface IWorkTimeDayJobRepository
  {
    Task<DbWorkTimeDayJob> GetAsync(Guid workTimeDayJobId, bool includeWorkTime = false);

    Task<Guid?> CreateAsync(DbWorkTimeDayJob dayJob);

    Task<bool> EditAsync(Guid workTimeDayJobId, JsonPatchDocument<DbWorkTimeDayJob> request);
  }
}
