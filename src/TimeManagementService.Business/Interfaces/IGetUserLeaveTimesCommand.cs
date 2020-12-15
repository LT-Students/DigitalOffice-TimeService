using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LT.DigitalOffice.TimeManagementService.Business.Interfaces
{
    /// <summary>
    /// Represents interface for a command in command pattern.
    /// Provides method for get user leave times.
    /// </summary>
    public interface IGetUserLeaveTimesCommand
    {
        /// <summary>
        /// Get leave time of user with id <param name="userId">.
        /// </summary>
        /// <param name="userId">ID of the user who is looking for data.</param>
        /// <param name="currentUserId">Id of the getting user.</param>
        /// <returns>Leave times with UserId <param name="userId">.</returns>
        public IEnumerable<LeaveTime> Execute(Guid userId, Guid currentUserId);
    }
}
