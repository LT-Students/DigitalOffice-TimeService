using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;

namespace LT.DigitalOffice.TimeService.Mappers.Db.Interfaces
{
  [AutoInject]
  public interface IDbWorkTimeDayJobMapper
  {
    DbWorkTimeDayJob Map(CreateWorkTimeDayJobRequest request);
  }
}
