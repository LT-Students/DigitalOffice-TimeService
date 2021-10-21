using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTimeDayJob.Interfaces
{
  [AutoInject]
  public interface IEditWorkTimeDayJobCommand
  {
    Task<OperationResultResponse<bool>> ExecuteAsync(Guid workTimeDayJobId, JsonPatchDocument<EditWorkTimeDayJobRequest> request);
  }
}
