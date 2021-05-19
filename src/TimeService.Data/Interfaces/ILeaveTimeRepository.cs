using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Models.Db;
using System;
using System.Collections.Generic;

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

        List<DbLeaveTime> Find(FindLeaveTimesFilter filter, int skipPagesCount, int takeCount, out int totalCount);
    }
}
