using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces
{
    /// <summary>
    /// Represents interface for a command in command pattern.
    /// Provides method for adding a new leave time.
    /// </summary>
    [AutoInject]
    public interface ICreateLeaveTimeCommand
    {
        /// <summary>
        /// Adds a new leave time. Returns id of the added leave time.
        /// </summary>
        /// <param name="request">Leave time data.</param>
        /// <returns>Id of the added leave time.</returns>
        OperationResultResponse<Guid> Execute(CreateLeaveTimeRequest request);
    }
}
