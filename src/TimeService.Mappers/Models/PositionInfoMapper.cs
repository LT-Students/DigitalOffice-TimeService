using LT.DigitalOffice.Models.Broker.Models.Position;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
  public class PositionInfoMapper : IPositionInfoMapper
  {
    public PositionInfo Map(PositionData positionData)
    {
      if (positionData is null)
      {
        return null;
      }

      return new()
      {
        Name = positionData.Name, 
        Id = positionData.Id
      };
    }
  }
}
