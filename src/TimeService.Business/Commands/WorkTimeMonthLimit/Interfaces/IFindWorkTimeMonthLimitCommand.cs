using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTimeMonthLimit.Interfaces
{
    [AutoInject]
    public interface IFindWorkTimeMonthLimitCommand
    {
        FindResultResponse<WorkTimeMonthLimitInfo> Execute(
            FindWorkTimeMonthLimitsFilter filter,
            int skipCount,
            int takeCount);
    }
}
