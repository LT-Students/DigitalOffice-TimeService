namespace LT.DigitalOffice.TimeService.Business.Helpers.Workdays.Intergations.Interface
{
    public interface ICalendar
    {
        string GetWorkCalendarByMonth(int month, int year, bool includeCovidNonWorkingDays = false);
    }
}
