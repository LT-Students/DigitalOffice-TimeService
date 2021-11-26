using System.Threading.Tasks;

namespace LT.DigitalOffice.TimeService.Business.Helpers.Workdays.Intergations.Interface
{
  public interface ICalendar
  {
    Task<string> GetWorkCalendarByMonthAsync(int month, int year, bool includeCovidNonWorkingDays = false);
  }
}
