using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces
{
  [AutoInject]
  public interface ICreateWorkTimeCommand
  {
    Task<OperationResultResponse<Guid?>> ExecuteAsync(CreateWorkTimeRequest request);
  }
}
