using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LT.DigitalOffice.TimeService.Business.Helpers.LeaveTimes
{
  public class ProlongedLeaveTimeHelper
  {
    private readonly IServiceScopeFactory _scopeFactory;

    private async Task FindAndUpdateProlongedLeaveTimes()
    {
      using (var scope = _scopeFactory.CreateScope())
      {
        using (var dbContext = scope.ServiceProvider.GetRequiredService<TimeServiceDbContext>())
        {
          DateTime thisMonthFirstDay = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
          DateTime endTime = thisMonthFirstDay.AddMilliseconds(-1);

          List<DbLeaveTime> leaveTimes = await dbContext.LeaveTimes
            .Where(lt =>
              lt.LeaveType == (int)LeaveType.Prolonged
              && !lt.IsClosed
              && lt.EndTime < thisMonthFirstDay
              && lt.IsActive)
            .ToListAsync();

          foreach (DbLeaveTime leaveTime in leaveTimes)
          {
            leaveTime.EndTime = endTime.AddMonths(1);
          }

          await dbContext.SaveChangesAsync();
        }
      }
    }

    public ProlongedLeaveTimeHelper(
      IServiceScopeFactory scopeFactory)
    {
      _scopeFactory = scopeFactory;
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
            || DateTime.UtcNow.Month != lastSuccessfulAttempt.Month)
          {
            await FindAndUpdateProlongedLeaveTimes();

            previousAttempt = DateTime.UtcNow;
            lastSuccessfulAttempt = previousAttempt;
          }

          if (lastSuccessfulAttempt != previousAttempt)
          {
            await Task.Delay(TimeSpan.FromMinutes(minutesToRestartAfterError));
          }
          else
          {
            await Task.Delay(TimeSpan.FromHours(1));
          }
        }
      });
    }
  }
}
