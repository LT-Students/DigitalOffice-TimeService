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
  public interface ILeaveTimeRepository
  {
    /// <summary>
    /// Adds new leave time to the database. Returns the id of the added leave time.
    /// </summary>
    /// <param name="leaveTime">Leave time to add.</param>
    /// <returns>Id of the added leave time.</returns>
    Guid Add(DbLeaveTime leaveTime);

    List<DbLeaveTime> Find(FindLeaveTimesFilter filter, out int totalCount);

    List<DbLeaveTime> Find(List<Guid> usersIds, int year, int month);

    DbLeaveTime Get(Guid leaveTimeId);

    bool Edit(DbLeaveTime leaveTime, JsonPatchDocument<DbLeaveTime> request);

    bool HasOverlap(DbLeaveTime leaveTime, DateTime? newStart, DateTime? newEnd);

    bool HasOverlap(Guid userId, DateTime start, DateTime end);
  }
}
