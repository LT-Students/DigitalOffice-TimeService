using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;

namespace LT.DigitalOffice.TimeService.Business.Helpers.Workdays.Intergations.Interface
{
  [AutoInject]
  public interface ICalendar
  {
    Task<string> GetWorkCalendarByMonthAsync(int month, int year, bool includeCovidNonWorkingDays = false);
  }
}
