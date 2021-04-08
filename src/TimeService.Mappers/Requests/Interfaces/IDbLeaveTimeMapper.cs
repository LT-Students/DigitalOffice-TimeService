using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Mappers.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto;

namespace LT.DigitalOffice.TimeService.Mappers.Requests.Interfaces
{
    [AutoInject]
    public interface IDbLeaveTimeMapper : IMapper<CreateLeaveTimeRequest, DbLeaveTime>
    {
    }
}
