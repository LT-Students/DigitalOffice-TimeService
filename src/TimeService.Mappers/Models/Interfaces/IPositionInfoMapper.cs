using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models.Position;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models.Interfaces
{
  [AutoInject]
  public interface IPositionInfoMapper
  {
    public PositionInfo Map(PositionData positionData);
  }
}
