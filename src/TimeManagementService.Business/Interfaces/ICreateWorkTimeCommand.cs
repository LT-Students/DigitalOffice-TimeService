using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using System;

namespace LT.DigitalOffice.TimeManagementService.Business.Interfaces
{
    /// <summary>
    /// Represents interface for a command in command pattern.
    /// Provides method for adding a new work time.
    /// </summary>
    public interface ICreateWorkTimeCommand
    {
        /// <summary>
        /// Adds a new work time. Returns id of the added work time.
        /// </summary>
        /// <param name="request">Work time data.</param>
        /// <param name="currentUserId">Id of the creating user.</param>
        /// <returns>Id of the added work time.</returns>
        public Guid Execute(WorkTimeRequest request, Guid currentUserId);
    }
}
