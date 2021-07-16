﻿using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces
{
    /// <summary>
    /// Represents interface for a command in command pattern.
    /// Provides method for adding a new work time.
    /// </summary>
    [AutoInject]
    public interface ICreateWorkTimeCommand
    {
        /// <summary>
        /// Adds a new work time. Returns id of the added work time.
        /// </summary>
        /// <param name="request">Work time data.</param>
        /// <returns>Id of the added work time.</returns>
        OperationResultResponse<Guid> Execute(CreateWorkTimeRequest request);
    }
}
