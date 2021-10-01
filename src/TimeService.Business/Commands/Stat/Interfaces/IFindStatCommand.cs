using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Business.Commands.Stat.Interfaces
{
  [AutoInject]
  public interface IFindStatCommand
  {
    Task<FindResultResponse<StatInfo>> Execute(FindStatFilter filter);
  }
}
