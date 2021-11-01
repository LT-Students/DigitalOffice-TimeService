using System;
using System.Linq;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;

namespace LT.DigitalOffice.TimeService.Mappers.Db
{
  public class DbWorkTimeMonthLimitMapper : IDbWorkTimeMonthLimitMapper
  {
    public DbWorkTimeMonthLimit Map(int year, int month, string holidays, int workingDayDuration)
    {
      if (holidays == null)
      {
        return null;
      }

      return new()
      {
        Id = Guid.NewGuid(),
        ModifiedAtUtc = DateTime.UtcNow,
        Holidays = holidays,
        ModifiedBy = null,
        Month = month,
        Year = year,
        NormHours = holidays.ToCharArray().Count(h => h == '0') * workingDayDuration
      };
    }
  }
}
