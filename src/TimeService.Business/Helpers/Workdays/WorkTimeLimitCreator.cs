using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LT.DigitalOffice.TimeService.Business.Helpers.Workdays.Intergations.Interface;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Business.Helpers.Workdays
{
  public class WorkTimeLimitCreator
  {
    private const int WorkingDayDuration = 8;

    private readonly ICalendar _calendar;
    private readonly ILogger<WorkTimeLimitCreator> _logger;
    private readonly IDbWorkTimeMonthLimitMapper _mapper;
    private readonly IWorkTimeMonthLimitRepository _limitRepository;

    private int _countNeededNextMonth;
    private int _minutesToRestart;
    private int _lastSuccessfullySavedMonth;

    private async Task<string> GetNonWorkingDaysAsync(int month, int year)
    {
      try
      {
        return await _calendar.GetWorkCalendarByMonthAsync(month, year);
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, "Cannot get non-working days.");

        return null;
      }
    }

    private async Task ExecuteAsync()
    {
      DbWorkTimeMonthLimit last = await _limitRepository.GetLastAsync();

      DateTime now = DateTime.UtcNow;

      _lastSuccessfullySavedMonth = last == null
        ? now.Year * 12 + now.Month - 1
        : last.Year * 12 + last.Month;

      int lastNeededMonth = now.Year * 12 + now.Month + _countNeededNextMonth;

      List<DbWorkTimeMonthLimit> limits = new();

      for (int month = _lastSuccessfullySavedMonth + 1; month <= lastNeededMonth; month++)
      {
        int requestYear = month % 12 == 0 ? month / 12 - 1 : month / 12;
        int requestMonth = month - requestYear * 12;

        string holidays = await GetNonWorkingDaysAsync(requestMonth, requestYear);

        if (string.IsNullOrEmpty(holidays))
        {
          break;
        }

        limits.Add(_mapper.Map(requestYear, requestMonth, holidays, WorkingDayDuration));
        _lastSuccessfullySavedMonth = month;
      }

      if (limits.Any())
      {
        await _limitRepository.CreateRangeAsync(limits);
      }
    }

    public WorkTimeLimitCreator(
      IWorkTimeMonthLimitRepository limitRepository,
      ILogger<WorkTimeLimitCreator> logger,
      IDbWorkTimeMonthLimitMapper mapper)
    {
      _limitRepository = limitRepository;
      _calendar = new IsDayOffIntegration();
      _logger = logger;
      _mapper = mapper;
    }

    public void Start(
      int minutesToRestartAfterError,
      int countNeededNextMonth)
    {
      _minutesToRestart = minutesToRestartAfterError;
      _countNeededNextMonth = countNeededNextMonth;

      // rework
      Thread.Sleep(30000);

      Task.Run(async () =>
      {
        while (true)
        {
          int neededMonths = DateTime.UtcNow.Year * 12 + DateTime.UtcNow.Month + _countNeededNextMonth;

          if (_lastSuccessfullySavedMonth == default
            || _lastSuccessfullySavedMonth < neededMonths)
          {
            await ExecuteAsync();
          }

          if (_lastSuccessfullySavedMonth == neededMonths)
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
