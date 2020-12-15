using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using System;

namespace LT.DigitalOffice.TimeManagementService.Business.Interfaces
{
    /// <summary>
    /// Represents interface for a command in command pattern.
    /// Provides method for change work time.
    /// </summary>
    public interface IEditWorkTimeCommand
    {
        /// <summary>
        /// Changes a work time. Returns true if the operation is successful.
        /// </summary>
        /// <param name="request">Work time data with id and changes.</param>
        /// <param name="currentUserId">Id of the ediding user.</param>
        /// <returns>True if the operation is successful.</returns>
        public bool Execute(EditWorkTimeRequest request, Guid currentUserId);
    }
}
