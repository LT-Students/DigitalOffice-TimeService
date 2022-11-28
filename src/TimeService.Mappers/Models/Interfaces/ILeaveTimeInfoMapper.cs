using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models.Interfaces
{
    [AutoInject]
    public interface ILeaveTimeInfoMapper
    {
        LeaveTimeInfo Map(DbLeaveTime dbLeaveTime, UserInfo managerInfo);
    }
}
