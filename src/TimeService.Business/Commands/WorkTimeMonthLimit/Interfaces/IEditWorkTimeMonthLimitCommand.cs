using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTimeMonthLimit.Interfaces
{
    [AutoInject]
    public interface IEditWorkTimeMonthLimitCommand
    {
        OperationResultResponse<bool> Execute(
            Guid workTimeMonthLimitId,
            JsonPatchDocument<EditWorkTimeMonthLimitRequest> request);
    }
}
