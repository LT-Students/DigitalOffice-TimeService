using LT.DigitalOffice.Kernel.Broker;
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

        public int StartDayOfMonth { get; private set; }
        public int CountOfMinutesToRestartAfterError { get; private set; }
        public DateTime LastSuccessfulAttempt { get; private set; }
        public DateTime PreviousAttempt { get; private set; }

        private Dictionary<Guid, List<Guid>> GetProjectsUsers()
        {
            const string logMessage = "Cannot get projects users.";

            try
            {
                var response = _rcProjectsUsers.GetResponse<IOperationResult<IGetProjectsUsersResponse>>(
                    IGetProjectsUsersRequest.CreateObj()).Result;

                if (response.Message.IsSuccess)
                {
                    return response.Message.Body.ProjectUsers;
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

            Dictionary<Guid, List<Guid>> projectsUsers = GetProjectsUsers();

            if (projectsUsers == null)
            {
                PreviousAttempt = DateTime.UtcNow;
                return false;
            }

            foreach (var pair in projectsUsers)
            {
                foreach (var userId in pair.Value)
                {
                    _workTimeRepository.Create(
                        new DbWorkTime
                        {
                            Id = Guid.NewGuid(),
                            Month = time.Month,
                            Year = time.Year,
                            ProjectId = pair.Key,
                            UserId = userId
                        });
                }
            }

            PreviousAttempt = DateTime.UtcNow;
            LastSuccessfulAttempt = PreviousAttempt;
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
