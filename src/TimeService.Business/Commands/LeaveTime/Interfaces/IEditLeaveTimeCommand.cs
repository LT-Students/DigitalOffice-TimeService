using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces
{
    [AutoInject]
    public interface IEditLeaveTimeCommand
    {
        OperationResultResponse<bool> Execute(Guid leaveTimeId, JsonPatchDocument<EditLeaveTimeRequest> request);
    }
}
