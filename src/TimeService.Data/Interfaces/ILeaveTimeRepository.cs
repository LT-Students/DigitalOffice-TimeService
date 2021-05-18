﻿using LT.DigitalOffice.Kernel.Attributes;
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
        /// Returns the leave times of specified id of user from database.
        /// </summary>
        /// <param name="userId">Id of user.</param>
        /// <returns>Leave times suitable for the specified parameters.</returns>
        ICollection<DbLeaveTime> GetUserLeaveTimes(Guid userId);

        /// <summary>
        /// Adds new leave time to the database. Returns the id of the added leave time.
        /// </summary>
        /// <param name="leaveTime">Leave time to add.</param>
        /// <returns>Id of the added leave time.</returns>
        Guid CreateLeaveTime(DbLeaveTime leaveTime);
    }
}
