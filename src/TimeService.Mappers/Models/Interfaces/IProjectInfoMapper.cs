using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models.Interfaces
{
    [AutoInject]
    public interface IProjectInfoMapper
    {
        ProjectInfo Map(ProjectData project);
    }
}
