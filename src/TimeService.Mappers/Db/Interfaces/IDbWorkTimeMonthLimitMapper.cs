using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;

namespace LT.DigitalOffice.TimeService.Mappers.Db.Interfaces
{
  [AutoInject]
  public interface IDbWorkTimeMonthLimitMapper
  {
    DbWorkTimeMonthLimit Map(int year, int month, string holidays, int workingDayDuration);
  }
}
