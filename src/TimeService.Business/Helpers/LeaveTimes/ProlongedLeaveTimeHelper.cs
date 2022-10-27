using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Business.Helpers.Workdays;
using LT.DigitalOffice.TimeService.Business.Helpers.Workdays.Intergations.Interface;
using LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Business.Helpers.LeaveTimes
{
  public class ProlongedLeaveTimeHelper
  {
    private const int WorkingDayDurationInHours = 8;
    private const int MinutesInHourCount = 60;
    private const int FirstDayInMonth = 1;
    private const int DefaultUpdateRateInHours = 24;
    private const double DefaultRate = 1;

    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ILogger<ProlongedLeaveTimeHelper> _logger;
    private readonly ICalendar _calendar;
    private readonly ICompanyService _companyService;
    private readonly IDbWorkTimeMonthLimitMapper _mapper;

    private async Task<Dictionary<DateTime, DbWorkTimeMonthLimit>> GetMonthLimitsDictionaryAsync(
      TimeServiceDbContext dbContext,
      DateTime startDate,
      DateTime endDate)
    {
      Dictionary<DateTime, DbWorkTimeMonthLimit> monthLimitsDictionary = (await dbContext.WorkTimeMonthLimits
        .Where(ml => ml.Year == startDate.Year && ml.Month >= startDate.Month
          || ml.Year == endDate.Year && ml.Month <= endDate.Month
          || ml.Year > startDate.Year && ml.Year < endDate.Year)
        .OrderBy(ml => ml.Year).ThenBy(ml => ml.Month)
        .ToListAsync())
        .ToDictionary(ml => new DateTime(ml.Year, ml.Month, FirstDayInMonth));

      DateTime startDateFirstDay = new(startDate.Year, startDate.Month, FirstDayInMonth);
      DateTime endDateFirstDay = new(endDate.Year, endDate.Month, FirstDayInMonth);

      for (DateTime dateTime = startDateFirstDay; dateTime <= endDateFirstDay; dateTime = dateTime.AddMonths(1))
      {
        if (!monthLimitsDictionary.ContainsKey(dateTime))
        {
          _logger.LogError($"Can't find WorkTimeMonthLimit with month: {dateTime.Month}, year: {dateTime.Year} in database.");

          var ml = _mapper.Map(
              year: dateTime.Year,
              month: dateTime.Month,
              holidays: await _calendar.GetWorkCalendarByMonthAsync(
                month: dateTime.Month,
                year: dateTime.Year),
              workingDayDuration: WorkingDayDurationInHours);

          monthLimitsDictionary.Add(
            key: dateTime,
            value: ml);
        }
      }

      return monthLimitsDictionary;
    }

    private async Task FindAndUpdateProlongedLeaveTimes()
    {
      using (var scope = _scopeFactory.CreateScope())
      {
        using (TimeServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<TimeServiceDbContext>())
        {
          DateTime thisMonthFirstDay = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, FirstDayInMonth);

          List<DbLeaveTime> leaveTimes = (await dbContext.LeaveTimes
            .Where(lt => lt.ParentId == null).Include(lt => lt.ManagerLeaveTime)
            .Where(lt =>
              (lt.ManagerLeaveTime == null
                && lt.LeaveType == (int)LeaveType.Prolonged
                && !lt.IsClosed
                && lt.EndTime < thisMonthFirstDay
                && lt.IsActive)
              || (lt.ManagerLeaveTime != null
                && lt.ManagerLeaveTime.LeaveType == (int)LeaveType.Prolonged
                && !lt.ManagerLeaveTime.IsClosed
                && lt.ManagerLeaveTime.EndTime < thisMonthFirstDay
                && lt.ManagerLeaveTime.IsActive))
            .ToListAsync())
            .Select(lt => lt.ManagerLeaveTime ?? lt)
            .OrderBy(lt => lt.EndTime.Year).ThenBy(lt => lt.EndTime.Month)
            .ToList();

          if (!leaveTimes.Any())
          {
            return;
          }

          // TODO - add new consumer to company service for searching only needed users
          Task<List<CompanyData>> companyUsersTask = _companyService.GetCompaniesDataAsync(
            usersIds: leaveTimes.Select(x => x.UserId).ToList(),
            errors: default);

          Dictionary<DateTime, DbWorkTimeMonthLimit> limitsDictionary = await GetMonthLimitsDictionaryAsync(dbContext, leaveTimes.First().EndTime.AddMonths(1), thisMonthFirstDay);
          Dictionary<Guid, double?> usersRatesDictionary = (await companyUsersTask)?.SelectMany(c => c.Users).ToDictionary(cu => cu.UserId, cu => cu.Rate);

          if (usersRatesDictionary is null)
          {
            _logger.LogError("Can't get user's rate, default values will be used.");
          }

          DateTime newEndTime = thisMonthFirstDay.AddMonths(1).AddMilliseconds(-1);

          foreach (DbLeaveTime leaveTime in leaveTimes)
          {
            decimal totalHours = 0m;

            for (DateTime dateTime = leaveTime.EndTime.AddMonths(1); dateTime < thisMonthFirstDay.AddMonths(1); dateTime = dateTime.AddMonths(1))
            {
              totalHours += (decimal)limitsDictionary[new DateTime(dateTime.Year, dateTime.Month, FirstDayInMonth)].NormHours;
            }

            leaveTime.EndTime = newEndTime;

            double? rate = DefaultRate;
            usersRatesDictionary?.TryGetValue(leaveTime.UserId, out rate);

            leaveTime.Minutes += (int)((decimal)(rate ?? DefaultRate) * totalHours * MinutesInHourCount);
          }

          await dbContext.SaveChangesAsync();
        }
      }
    }

    public ProlongedLeaveTimeHelper(
      ILogger<ProlongedLeaveTimeHelper> logger,
      IServiceScopeFactory scopeFactory,
      ICalendar calendar,
      ICompanyService companyService,
      IDbWorkTimeMonthLimitMapper mapper)
    {
      _logger = logger;
      _scopeFactory = scopeFactory;
      _calendar = calendar;
      _companyService = companyService;
      _mapper = mapper;
    }

    public void Start(
      int minutesToRestartAfterError,
      DateTime lastSuccessfulAttempt)
    {
      DateTime previousAttempt = default;

      Task.Run(async () =>
      {
        while (true)
        {
          if (lastSuccessfulAttempt == default
            || DateTime.UtcNow.Day != lastSuccessfulAttempt.Day)
          {
            try
            {
              await FindAndUpdateProlongedLeaveTimes();

              previousAttempt = DateTime.UtcNow;
              lastSuccessfulAttempt = previousAttempt;
            }
            catch (Exception ex)
            {
              _logger.LogError(ex, "An exception occurred while trying to update prolonged leave times.");
              previousAttempt = DateTime.UtcNow;
            }
          }

          if (lastSuccessfulAttempt != previousAttempt)
          {
            await Task.Delay(TimeSpan.FromMinutes(minutesToRestartAfterError));
          }
          else
          {
            await Task.Delay(TimeSpan.FromHours(DefaultUpdateRateInHours));
          }
        }
      });
    }
  }
}
