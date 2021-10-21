using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTimeMonthLimit.Interfaces
{
  [AutoInject]
  public interface IEditWorkTimeMonthLimitCommand
  {
    Task<OperationResultResponse<bool>> ExecuteAsync(
      Guid workTimeMonthLimitId,
      JsonPatchDocument<EditWorkTimeMonthLimitRequest> request);
  }
}
