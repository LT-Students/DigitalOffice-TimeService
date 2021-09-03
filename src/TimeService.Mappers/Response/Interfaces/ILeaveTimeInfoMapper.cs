using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;

namespace LT.DigitalOffice.TimeService.Mappers.Response.Interfaces
{
  [AutoInject]
  public interface ILeaveTimeResponseMapper
  {
    LeaveTimeResponse Map(DbLeaveTime dbLeaveTime, UserInfo user);
  }
}
