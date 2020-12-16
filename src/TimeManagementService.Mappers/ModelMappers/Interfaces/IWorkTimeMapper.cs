using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Responses;

namespace LT.DigitalOffice.TimeManagementService.Mappers.ModelMappers.Interfaces
{
    public interface IWorkTimeMapper : IMapper<WorkTimeRequest, DbWorkTime>, IMapper<DbWorkTime, WorkTimeResponse>
    {
    }
}
