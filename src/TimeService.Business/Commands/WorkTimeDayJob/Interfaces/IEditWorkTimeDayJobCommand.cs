using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTimeDayJob.Interfaces
{
    [AutoInject]
    public interface IEditWorkTimeDayJobCommand
    {
        OperationResultResponse<bool> Execute(Guid workTimeDayJobId, JsonPatchDocument<EditWorkTimeDayJobRequest> request);
    }
}
