using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces
{
    [AutoInject]
    public interface IFindLeaveTimesCommand
    {
        FindResultResponse<LeaveTimeInfo> Execute(FindLeaveTimesFilter filter, int skipPagesCount, int takeCount);
    }
}
