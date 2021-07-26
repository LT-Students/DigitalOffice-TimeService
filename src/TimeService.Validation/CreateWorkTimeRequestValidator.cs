using FluentValidation;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Validation
{
    public class CreateWorkTimeRequestValidator : AbstractValidator<CreateWorkTimeRequest>, ICreateWorkTimeRequestValidator
    {
        private readonly IRequestClient<IGetUserProjectsRequest> _rcGetUserProjects;
        private readonly ILogger<ICreateWorkTimeRequestValidator> _logger;

        private bool IsUserInProject(Guid userId, Guid projectId)
        {
            const string logMessage = "Cannot check including user '{userId}' to project '{projectId}'.";

            try
            {
                IOperationResult<IProjectsResponse> response = _rcGetUserProjects
                    .GetResponse<IOperationResult<IProjectsResponse>>(IGetUserProjectsRequest.CreateObj(userId)).Result.Message;

                if (response.IsSuccess && response.Body.ProjectsIds.Contains(projectId))
                {
                    return true;
                }

                _logger.LogWarning(
                    logMessage + "Reason:\n{Errors}",
                    userId,
                    projectId,
                    string.Join(',', response.Errors));
            }
            catch(Exception exc)
            {
                _logger.LogError(exc, logMessage, userId, projectId);
            }

            return false;
        }

        /// <summary>
        /// How many days ago can WorkTime be added.
        /// </summary>
        public const int FromDay = 3;
        /// <summary>
        /// How many days ahead can WorkTime be added.
        /// </summary>
        public const int ToDay = 2;
        /// <summary>
        /// Limit on working hours in a row.
        /// </summary>
        public static TimeSpan WorkingLimit { get; } = new TimeSpan(24, 0, 0);

        private readonly DateTime fromDateTime = DateTime.Now.AddDays(-FromDay);
        private readonly DateTime toDateTime = DateTime.Now.AddDays(ToDay);
        private readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en-GB");

        public CreateWorkTimeRequestValidator(
            IWorkTimeRepository repository,
            IRequestClient<IGetUserProjectsRequest> rcGetUserProjects,
            ILogger<ICreateWorkTimeRequestValidator> logger)
        {
            _rcGetUserProjects = rcGetUserProjects;
            _logger = logger;

            RuleFor(wt => wt.UserId)
                .NotEmpty();

            RuleFor(wt => wt.ProjectId)
                .NotEmpty();

            RuleFor(wt => wt)
                .Must(wt => IsUserInProject(wt.UserId, wt.ProjectId))
                .WithMessage("the user must participate in the project.");

            //RuleFor(wt => wt.Title)
            //    .NotEmpty()
            //    .MaximumLength(200)
            //    .WithMessage("Title is too long.");

            RuleFor(wt => wt.StartTime)
                .NotEqual(new DateTime());
            //    .Must(st => st > fromDateTime).WithMessage(date =>
            //        $"WorkTime had to be filled no later than {fromDateTime.ToString(culture)}.")
            //    .Must(st => st < toDateTime)
            //    .WithMessage(date => $"WorkTime cannot be filled until {toDateTime.ToString(culture)}.");

            RuleFor(wt => wt.EndTime)
                .NotEqual(new DateTime());

            RuleFor(wt => wt)
                .Must(wt => wt.StartTime < wt.EndTime)
                .WithMessage("Start time must be before end time.");
            //    .Must(wt => wt.EndTime - wt.StartTime <= WorkingLimit).WithMessage(time =>
            //        $"You cannot indicate that you worked more than {WorkingLimit.Hours} hours and {WorkingLimit.Minutes} minutes.")
            //    .Must(wt =>
            //    {
            //        var oldWorkTimes = repository.Find(
            //            new FindWorkTimesFilter
            //            {
            //                UserId = wt.UserId,
            //                StartTime = wt.StartTime.AddMinutes(-WorkingLimit.TotalMinutes),
            //                EndTime = wt.EndTime
            //            },
            //            0,
            //            int.MaxValue,
            //            out _);

            //        return oldWorkTimes.All(oldWorkTime =>
            //            wt.EndTime <= oldWorkTime.StartTime || oldWorkTime.EndTime <= wt.StartTime);
            //    }).WithMessage("New WorkTime should not overlap with old ones.");
        }
    }
}
