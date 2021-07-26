using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces
{
    [AutoInject]
    public interface IFindWorkTimesCommand
    {
        FindResultResponse<WorkTimeInfo> Execute(FindWorkTimesFilter filter, int skipPagesCount, int takeCount);
    }
}
