using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTimeMonthLimit.Interfaces
{
  [AutoInject]
  public interface IFindWorkTimeMonthLimitCommand
  {
    Task<FindResultResponse<WorkTimeMonthLimitInfo>> ExecuteAsync(FindWorkTimeMonthLimitsFilter filter);
  }
}
