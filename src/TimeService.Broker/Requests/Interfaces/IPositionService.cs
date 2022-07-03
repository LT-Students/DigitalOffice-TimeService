using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models.Position;

namespace LT.DigitalOffice.TimeService.Broker.Requests.Interfaces
{
  [AutoInject]
  public interface IPositionService
  {
    Task<List<PositionData>> GetPositionsAsync(List<Guid> usersIds, List<string> errors);
  }
}
