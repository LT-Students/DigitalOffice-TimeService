using FluentValidation;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Broker.Responses;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Exceptions;
using LT.DigitalOffice.TimeManagementService.Data.Filters;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;

namespace LT.DigitalOffice.TimeManagementService.Validation
{
    public class WorkTimeValidator : AbstractValidator<WorkTime>
    {
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

        public WorkTimeValidator(
            [FromServices] IWorkTimeRepository repository,
            [FromServices] IAssignValidator assignValidator)
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .DependentRules(() =>
                {
                    RuleFor(x => x)
                    .Must(x => assignValidator.CanAssignUser(x.CurrentUserId, x.UserId))
                    .WithMessage("You cannot assign this user.");
                })
                .WithMessage("User does not exist.");

            RuleFor(x => x.StartDate)
                .NotEqual(new DateTime())
                .Must(st => st > fromDateTime).WithMessage(date =>
                    $"WorkTime had to be filled no later than {fromDateTime.ToString(culture)}.")
                .Must(st => st < toDateTime)
                .WithMessage(date => $"WorkTime cannot be filled until {toDateTime.ToString(culture)}.");

            RuleFor(x => x.EndDate)
                .NotEqual(new DateTime());

            RuleFor(x => x.ProjectId)
                .NotEmpty()
                .DependentRules(() =>
                {
                    RuleFor(x => x)
                    .Must(x => assignValidator.CanAssignUser(x.UserId, x.ProjectId))
                    .WithMessage("You cannot assign this user on this project.");
                })
                .WithMessage("Project does not exist."); ;

            RuleFor(x => x.Title)
                .NotEmpty();

            RuleFor(x => x)
                .Must(x => x.StartDate < x.EndDate)
                .WithMessage("You cannot indicate that you worked zero hours or a negative amount.")
                .Must(x => x.EndDate - x.StartDate <= WorkingLimit)
                .WithMessage(x =>
                    $"You cannot indicate that you worked more than {WorkingLimit.Hours} hours and {WorkingLimit.Minutes} minutes.")
                .Must(x =>
                {
                    var oldWorkTimes = repository.GetUserWorkTimes(
                        x.UserId,
                        new WorkTimeFilter
                        {
                            StartTime = x.StartDate.AddMinutes(-WorkingLimit.TotalMinutes),
                            EndTime = x.EndDate
                        });

                    return oldWorkTimes.All(oldWorkTime =>
                        x.EndDate <= oldWorkTime.StartDate || oldWorkTime.EndDate <= x.StartDate);
                }).WithMessage("New WorkTime should not overlap with old ones.");
        }
    }
}
