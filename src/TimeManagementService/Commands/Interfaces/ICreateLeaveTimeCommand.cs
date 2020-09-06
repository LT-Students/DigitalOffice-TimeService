using LT.DigitalOffice.TimeManagementService.Models;
using System;

namespace LT.DigitalOffice.TimeManagementService.Commands.Interfaces
{
    /// <summary>
    /// Represents interface for a command in command pattern.
    /// Provides method for adding a new leave time.
    /// </summary>
    public interface ICreateLeaveTimeCommand
    {
        /// <summary>
        /// Adds a new leave time. Returns id of the added leave time.
        /// </summary>
        /// <param name="request">Leave time data.</param>
        /// <returns>Id of the added leave time.</returns>
        Guid Execute(CreateLeaveTimeRequest request);
    }
}
