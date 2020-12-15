using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using System;

namespace LT.DigitalOffice.TimeManagementService.Business.Interfaces
{
    /// <summary>
    /// Represents interface for a command in command pattern.
    /// Provides method for change leave time.
    /// </summary>
    public interface IEditLeaveTimeCommand
    {
        /// <summary>
        /// Changes a leave time. Returns true if the operation is successful.
        /// </summary>
        /// <param name="request">Leave time data with id and changes.</param>
        /// <param name="currentUserId">Id of the editing user.</param>
        /// <returns>True if the operation is successful.</returns>
        public bool Execute(EditLeaveTimeRequest request, Guid currentUserId);
    }
}
