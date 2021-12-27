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
  public interface ILeaveTimeRepository
  {
    Task<Guid?> CreateAsync(DbLeaveTime leaveTime);

    Task<(List<DbLeaveTime>, int totalCount)> FindAsync(FindLeaveTimesFilter filter);

    Task<List<DbLeaveTime>> GetAsync(List<Guid> usersIds, int year, int month);

    Task<DbLeaveTime> GetAsync(Guid leaveTimeId);

    Task<bool> EditAsync(DbLeaveTime leaveTime, JsonPatchDocument<DbLeaveTime> request);

    Task<bool> HasOverlapAsync(DbLeaveTime leaveTime, DateTime start, DateTime end);

    Task<bool> HasOverlapAsync(Guid userId, DateTime start, DateTime end);
  }
}
