using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LT.DigitalOffice.TimeService.Business.Helpers.Workdays
{
    public class WorkTimeCreater
    {
        private readonly IWorkTimeRepository _workTimeRepository;
        private readonly IRequestClient<IGetProjectsUsersRequest> _rcProjectsUsers;
        private readonly ILogger<WorkTimeCreater> _logger;

        private int _minutesToRestart;
        private DateTime _lastSuccessfulAttempt;
        private DateTime _previousAttempt;

        private List<ProjectUserData> GetProjectsUsers()
        {
            const string logMessage = "Cannot get projects users.";

            try
            {
                var response = _rcProjectsUsers.GetResponse<IOperationResult<IGetProjectsUsersResponse>>(
                    IGetProjectsUsersRequest.CreateObj()).Result;

                if (response.Message.IsSuccess)
                {
                    return response.Message.Body.Users;
                }

                _logger.LogWarning(logMessage);
            }
            catch(Exception exc)
            {
                _logger.LogError(exc, logMessage);
            }

            return null;
        }

        private bool Execute()
        {
            DateTime time = DateTime.UtcNow;

            List<ProjectUserData> projectsUsers = GetProjectsUsers();

            if (projectsUsers == null)
            {
                _previousAttempt = DateTime.UtcNow;
                return false;
            }

            foreach (var user in projectsUsers)
            {
                _workTimeRepository.Create(
                    new DbWorkTime
                    {
                        Id = Guid.NewGuid(),
                        Month = time.Month,
                        Year = time.Year,
                        ProjectId = user.ProjectId,
                        UserId = user.UserId
                    });
            }

            _previousAttempt = DateTime.UtcNow;
            _lastSuccessfulAttempt = _previousAttempt;
            return true;
        }

        public WorkTimeCreater(
            IWorkTimeRepository workTimeRepository,
            IRequestClient<IGetProjectsUsersRequest> rcProjectsUsers,
            ILogger<WorkTimeCreater> logger)
        {
            _workTimeRepository = workTimeRepository;
            _rcProjectsUsers = rcProjectsUsers;
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
