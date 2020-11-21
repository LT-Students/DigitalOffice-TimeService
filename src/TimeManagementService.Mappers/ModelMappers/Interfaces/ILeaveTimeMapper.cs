using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeManagementService.Mappers.ModelMappers.Interfaces
{
    public interface ILeaveTimeMapper : IMapper<LeaveTime, DbLeaveTime>, IMapper<DbLeaveTime, LeaveTime>
    {
    }
}
