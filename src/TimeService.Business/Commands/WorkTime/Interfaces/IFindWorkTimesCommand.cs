using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces
{
    [AutoInject]
    public interface IFindWorkTimesCommand
    {
        WorkTimesResponse Execute(FindWorkTimesFilter filter, int skipCount, int takeCount);
    }
}
