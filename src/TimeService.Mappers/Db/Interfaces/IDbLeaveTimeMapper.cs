using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;

namespace LT.DigitalOffice.TimeService.Mappers.Db.Interfaces
{
  [AutoInject]
  public interface IDbLeaveTimeMapper
  {
    DbLeaveTime Map(CreateLeaveTimeRequest request, double? rate = null, string holidays = null);
  }
}
