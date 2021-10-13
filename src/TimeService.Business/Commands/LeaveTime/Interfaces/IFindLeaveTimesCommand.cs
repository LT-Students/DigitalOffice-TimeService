using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces
{
  [AutoInject]
  public interface IFindLeaveTimesCommand
  {
    Task<FindResultResponse<LeaveTimeResponse>> ExecuteAsync(FindLeaveTimesFilter filter);
  }
}
