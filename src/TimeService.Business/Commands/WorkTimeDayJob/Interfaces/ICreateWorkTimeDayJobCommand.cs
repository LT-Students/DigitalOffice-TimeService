using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTimeDayJob.Interfaces
{
  [AutoInject]
  public interface ICreateWorkTimeDayJobCommand
  {
    Task<OperationResultResponse<Guid>> ExecuteAsync(CreateWorkTimeDayJobRequest request);
  }
}
