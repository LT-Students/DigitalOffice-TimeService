using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeService.Models.Db;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Business.Helpers.Workdays
{
  public class WorkTimeCreator
  {
    private readonly IRequestClient<IGetProjectsUsersRequest> _rcProjectsUsers;
    private readonly ILogger<WorkTimeCreator> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private int _minutesToRestart;
    private DateTime _lastSuccessfulAttempt;
    private DateTime _previousAttempt;

    private async Task<List<ProjectUserData>> GetProjectsUsersAsync()
    {
      const string logMessage = "Cannot get projects users.";

      try
      {
        var response = await _rcProjectsUsers.GetResponse<IOperationResult<IGetProjectsUsersResponse>>(
          IGetProjectsUsersRequest.CreateObj());

        if (response.Message.IsSuccess)
        {
          return response.Message.Body.Users;
        }

        _logger.LogWarning(logMessage);
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logMessage);
      }

      return null;
    }

    private async Task<bool> ExecuteAsync()
    {
      using (var scope = _scopeFactory.CreateScope())
      {
        using (var dbContext = scope.ServiceProvider.GetRequiredService<TimeServiceDbContext>())
        {
          DateTime time = DateTime.UtcNow;

          List<ProjectUserData> projectsUsers = await GetProjectsUsersAsync();

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
      IRequestClient<IGetProjectsUsersRequest> rcProjectsUsers,
      ILogger<WorkTimeCreator> logger,
      IServiceScopeFactory scopeFactory)
    {
      _rcProjectsUsers = rcProjectsUsers;
      _logger = logger;
      _scopeFactory = scopeFactory;
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
