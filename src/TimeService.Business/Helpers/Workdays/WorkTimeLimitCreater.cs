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

        private int _minutesToRestart;
        private DateTime _lastSuccessfulAttempt;
        private DateTime _previousAttempt;

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
                _previousAttempt = DateTime.UtcNow;
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

            _previousAttempt = DateTime.UtcNow;
            _lastSuccessfulAttempt = _previousAttempt;
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
            int minutesToRestartAfterError,
            DateTime lastSuccessfulAttempt)
        {
            _minutesToRestart = minutesToRestartAfterError;
            _lastSuccessfulAttempt = lastSuccessfulAttempt;

            Task.Run(() =>
            {
                while (true)
                {
                    if (_lastSuccessfulAttempt == default
                    || DateTime.UtcNow.Month != _lastSuccessfulAttempt.Month)
                    {
                        Execute();
                    }

                    if (_lastSuccessfulAttempt != _previousAttempt)
                    {
                        Thread.Sleep(_minutesToRestart * 60000);
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
