using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
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
        /// <returns>Id of the added work time.</returns>
        Guid Execute(WorkTime request);
    }
}
