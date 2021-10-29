using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Data.Interfaces
{
  /// <summary>
  /// Represents interface of repository in repository pattern.
  /// Provides methods for working with the database of TimeService.
  /// </summary>
  [AutoInject]
  public interface IWorkTimeRepository
  {
    Task<Guid?> CreateAsync(DbWorkTime workTime);

    Task<DbWorkTime> GetAsync(Guid id);

    Task<bool> DoesExistAsync(Guid id);

    Task<(List<DbWorkTime>, int totalCount)> FindAsync(FindWorkTimesFilter filter);

    Task<List<DbWorkTime>> GetAsync(List<Guid> usersIds, List<Guid> projectsIds, int year, int month, bool includeJobs = false);

    Task<bool> EditAsync(DbWorkTime dbWorkTime, JsonPatchDocument<DbWorkTime> jsonPatchDocument);

    Task<DbWorkTime> GetLastAsync();
  }
}
