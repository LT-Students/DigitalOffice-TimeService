using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;

namespace LT.DigitalOffice.TimeService.Mappers.Response.Interfaces
{
  [AutoInject]
  public interface IWorkTimeResponseMapper
  {
    WorkTimeResponse Map(
        DbWorkTime dbWorkTime,
        DbWorkTimeMonthLimit dbMonthLimit,
        UserInfo userInfo,
        ProjectUserData projectUser,
        ProjectInfo project);
  }
}
