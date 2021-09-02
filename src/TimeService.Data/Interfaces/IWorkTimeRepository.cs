using System;
using System.Collections.Generic;
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
    Guid Create(DbWorkTime workTime);

    DbWorkTime Get(Guid id);

    bool Contains(Guid id);

    List<DbWorkTime> Find(FindWorkTimesFilter filter, out int totalCount);

    List<DbWorkTime> Find(List<Guid> usersIds, int year, int month, bool includeJobs = false);

    bool Edit(DbWorkTime dbWorkTime, JsonPatchDocument<DbWorkTime> jsonPatchDocument);

    DbWorkTime GetLast();
  }
}
