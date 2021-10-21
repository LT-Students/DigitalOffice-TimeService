using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces
{
  [AutoInject]
  public interface ICreateLeaveTimeCommand
  {
    Task<OperationResultResponse<Guid>> ExecuteAsync(CreateLeaveTimeRequest request);
  }
}
