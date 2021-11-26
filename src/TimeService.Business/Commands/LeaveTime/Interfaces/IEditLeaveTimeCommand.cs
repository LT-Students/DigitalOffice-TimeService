using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces
{
  [AutoInject]
  public interface IEditLeaveTimeCommand
  {
    Task<OperationResultResponse<bool>> ExecuteAsync(Guid leaveTimeId, JsonPatchDocument<EditLeaveTimeRequest> request);
  }
}
