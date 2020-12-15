using LT.DigitalOffice.TimeManagementService.Models.Db;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeManagementService.Data.Interfaces
{
    /// <summary>
    /// Represents interface of repository in repository pattern.
    /// Provides methods for working with the database of TimeManagementService.
    /// </summary>
    public interface ILeaveTimeRepository
    {
        /// <summary>
        /// Returns the leave times of specified id of user from database.
        /// </summary>
        /// <param name="userId">Id of user.</param>
        /// <returns>Leave times suitable for the specified parameters.</returns>
        public ICollection<DbLeaveTime> GetUserLeaveTimes(Guid userId);

        /// <summary>
        /// Adds new leave time to the database. Returns the id of the added leave time.
        /// </summary>
        /// <param name="leaveTime">Leave time to add.</param>
        /// <returns>Id of the added leave time.</returns>
        public Guid CreateLeaveTime(DbLeaveTime leaveTime);

        /// <summary>
        /// Change leave time in the database. Returns true if the operation is successful.
        /// </summary>
        /// <param name="leaveTime">New LeaveTime.</param>
        /// <returns>True if the operation is successful.</returns>
        public bool EditLeaveTime(DbLeaveTime leaveTime);

        /// <summary>
        /// Get leave time with specified id from the database.
        /// </summary>
        /// <param name="leaveTimeId">Leave time id.</param>
        /// <returns>Model with specified id.</returns>
        public DbLeaveTime GetLeaveTimeById(Guid leaveTimeId);
    }
}
