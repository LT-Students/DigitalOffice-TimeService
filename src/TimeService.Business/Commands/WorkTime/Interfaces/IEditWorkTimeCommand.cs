using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces
{

  [AutoInject]
  public interface IEditWorkTimeCommand
  {
    Task<OperationResultResponse<bool>> ExecuteAsync(Guid workTimeId, JsonPatchDocument<EditWorkTimeRequest> request);
  }
}
