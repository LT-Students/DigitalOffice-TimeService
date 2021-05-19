using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces
{
    /// <summary>
    /// Represents interface for a command in command pattern.
    /// Provides method for change work time.
    /// </summary>
    [AutoInject]
    public interface IEditWorkTimeCommand
    {
        /// <summary>
        /// Calls methods to edit the existing work time. Returns true if work time edited.
        /// </summary>
        /// <param name="workTimeId">Work time id to update the work time.</param>
        /// <param name="request">Data to update the work time.</param>
        /// <returns>True if the operation is successful.</returns>
        OperationResultResponse<bool> Execute(Guid workTimeId, JsonPatchDocument<EditWorkTimeRequest> request);
    }
}
