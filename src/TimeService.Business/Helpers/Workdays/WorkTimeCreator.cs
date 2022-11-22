using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.Extensions.DependencyInjection;

namespace LT.DigitalOffice.TimeService.Business.Helpers.Workdays
{
  public class WorkTimeCreator
  {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IProjectService _projectService;

    private int _minutesToRestart;
    private DateTime _lastSuccessfulAttempt;
    private DateTime _previousAttempt;

    private async Task<bool> ExecuteAsync()
    {
      using (var scope = _scopeFactory.CreateScope())
      {
        using (var dbContext = scope.ServiceProvider.GetRequiredService<TimeServiceDbContext>())
        {
          DateTime time = DateTime.UtcNow;

          List<ProjectUserData> projectsUsers = await _projectService.GetProjectsUsersAsync(isActive: true);

          if (projectsUsers == null)
          {
            _previousAttempt = DateTime.UtcNow;
            return false;
          }

          dbContext.WorkTimes.AddRange(
            projectsUsers.Select(pu => new DbWorkTime
            {
              Id = Guid.NewGuid(),
              Month = time.Month,
              Year = time.Year,
              ProjectId = pu.ProjectId,
              UserId = pu.UserId
            }));

          await dbContext.SaveChangesAsync();

          _previousAttempt = DateTime.UtcNow;
          _lastSuccessfulAttempt = _previousAttempt;

          return true;
        }
      }
    }

    public WorkTimeCreator(
      IServiceScopeFactory scopeFactory,
      IProjectService projectService)
    {
      _scopeFactory = scopeFactory;
      _projectService = projectService;
    }

    public void Start(
      int minutesToRestartAfterError,
      DateTime lastSuccessfulAttempt)
    {
      _minutesToRestart = minutesToRestartAfterError;
      _lastSuccessfulAttempt = lastSuccessfulAttempt;

      Task.Run(async () =>
      {
        while (true)
        {
          if (_lastSuccessfulAttempt == default
            || DateTime.UtcNow.Month != _lastSuccessfulAttempt.Month)
          {
            await ExecuteAsync();
          }

          if (_lastSuccessfulAttempt != _previousAttempt)
          {
            await Task.Delay(TimeSpan.FromMinutes(_minutesToRestart));
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
