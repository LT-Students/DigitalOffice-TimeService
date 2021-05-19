using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces
{
    [AutoInject]
    public interface IFindLeaveTimesCommand
    {
        LeaveTimesResponse Execute(FindLeaveTimesFilter filter, int skipPagesCount, int takeCount);
    }
}
