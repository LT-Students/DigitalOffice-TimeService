using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LT.DigitalOffice.TimeService.Business.Helpers.Workdays
{
    public class WorkTimeLimitCreater
    {
        private readonly IWorkTimeMonthLimitRepository _limitRepository;
        private readonly WorkCalendarHelper _calendarHelper;
        private readonly ILogger<WorkTimeLimitCreater> _logger;

        public int StartDayOfMonth { get; private set; }
        public int CountOfMinutesToRestartAfterError { get; private set; }
        public DateTime LastSuccessfulAttempt { get; private set; }
        public DateTime PreviousAttempt { get; private set; }

        private string GetNonWorkingDays(DateTime time)
        {
            const string logMessage = "Cannot get non-working days.";

            try
            {
                return _calendarHelper.GetWorkCalendarByMonth(time.Month, time.Year);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, logMessage);

                return null;
            }
        }

        private bool Execute()
        {
            DateTime time = DateTime.UtcNow;

            string holidays = GetNonWorkingDays(time);

            if (holidays == null)
            {
                PreviousAttempt = DateTime.UtcNow;
                return false;
            }

            DbWorkTimeMonthLimit limit = new()
            {
                Id = Guid.NewGuid(),
                ModifiedAtUtc = DateTime.UtcNow,
                Holidays = holidays,
                ModifiedBy = null,
                Month = time.Month,
                Year = time.Year,
                NormHours = holidays.ToCharArray().Count(h => h == '0') * 8
            };

            _limitRepository.Add(limit);

            PreviousAttempt = DateTime.UtcNow;
            LastSuccessfulAttempt = PreviousAttempt;
            return true;
        }

        public WorkTimeLimitCreater(
            IWorkTimeMonthLimitRepository limitRepository,
            ILogger<WorkTimeLimitCreater> logger)
        {
            _limitRepository = limitRepository;
            _calendarHelper = new();
            _logger = logger;
        }

        public void Start(
            int startDayOfMonth,
            int countOfMinutesToRestartAfterError,
            DateTime lastSuccessfulAttempt)
        {
            StartDayOfMonth = startDayOfMonth;
            CountOfMinutesToRestartAfterError = countOfMinutesToRestartAfterError;
            LastSuccessfulAttempt = lastSuccessfulAttempt;

            Task.Run(() =>
            {
                while (true)
                {
                    if (LastSuccessfulAttempt == default
                    || DateTime.UtcNow.Month != LastSuccessfulAttempt.Month && DateTime.UtcNow.Day >= StartDayOfMonth
                    || LastSuccessfulAttempt != PreviousAttempt)
                    {
                        Execute();
                    }

                    if (LastSuccessfulAttempt != PreviousAttempt)
                    {
                        Thread.Sleep(countOfMinutesToRestartAfterError * 60000);
                    }
                    else
                    {
                        Thread.Sleep(3600000);
                    }
                }
            });
        }
    }
}
