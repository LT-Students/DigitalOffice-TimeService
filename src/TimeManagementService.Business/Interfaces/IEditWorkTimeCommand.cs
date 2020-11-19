using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using Microsoft.AspNetCore.JsonPatch;

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
        /// <param name="request">Work time data.</param>
        /// <returns>True if the operation is successful.</returns>
        bool Execute(JsonPatchDocument<WorkTime> request);
    }
}
