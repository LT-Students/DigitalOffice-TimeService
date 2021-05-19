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
        DbWorkTime GetWorkTime(Guid id);

        /// <summary>
        /// Returns the filtered work times of specified id of user from database.
        /// </summary>
        /// <param name="userId">Id of user.</param>
        /// <param name="filter">Data restrictions.</param>
        /// <returns>Work times suitable for the specified parameters.</returns>
        ICollection<DbWorkTime> GetUserWorkTimes(Guid userId, WorkTimeFilter filter);

        /// <summary>
        /// Adds new work time to the database. Returns the id of the added work time.
        /// </summary>
        /// <param name="workTime">Work time to add.</param>
        /// <returns>Id of the added work time.</returns>
        Guid CreateWorkTime(DbWorkTime workTime);

        bool Edit(DbWorkTime dbWorkTime, JsonPatchDocument<DbWorkTime> jsonPatchDocument);
    }
}
