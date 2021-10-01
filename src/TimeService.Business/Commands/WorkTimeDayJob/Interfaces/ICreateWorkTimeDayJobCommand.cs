using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTimeDayJob.Interfaces
{
    [AutoInject]
    public interface ICreateWorkTimeDayJobCommand
    {
        OperationResultResponse<Guid> Execute(CreateWorkTimeDayJobRequest request);
    }
}
