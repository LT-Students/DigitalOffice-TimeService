using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models.Interfaces
{
  [AutoInject]
    public interface IWorkTimeInfoMapper
    {
        WorkTimeInfo Map(
            DbWorkTime dbWorkTime,
            ProjectInfo project,
            UserInfo manager);
    }
}
