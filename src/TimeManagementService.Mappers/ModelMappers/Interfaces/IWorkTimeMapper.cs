using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeManagementService.Mappers.ModelMappers.Interfaces
{
    public interface IWorkTimeMapper : IMapper<WorkTime, DbWorkTime>, IMapper<DbWorkTime, WorkTime>
    {
    }
}
