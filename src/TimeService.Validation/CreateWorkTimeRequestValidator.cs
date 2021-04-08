using FluentValidation;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Validation
{
    public class CreateWorkTimeRequestValidator : AbstractValidator<CreateWorkTimeRequest>, ICreateWorkTimeRequestValidator
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

        public CreateWorkTimeRequestValidator(IWorkTimeRepository repository)
        {
            RuleFor(wt => wt.WorkerUserId)
                    .NotEmpty();

            RuleFor(wt => wt.StartTime)
                .NotEqual(new DateTime())
                .Must(st => st > fromDateTime).WithMessage(date =>
                    $"WorkTime had to be filled no later than {fromDateTime.ToString(culture)}.")
                .Must(st => st < toDateTime)
                .WithMessage(date => $"WorkTime cannot be filled until {toDateTime.ToString(culture)}.");

            RuleFor(wt => wt.EndTime)
                .NotEqual(new DateTime());

            RuleFor(wt => wt.ProjectId)
                .NotEmpty();

            RuleFor(wt => wt.Title)
                .NotEmpty();

            RuleFor(wt => wt)
                .Must(wt => wt.StartTime < wt.EndTime)
                .WithMessage("You cannot indicate that you worked zero hours or a negative amount.")
                .Must(wt => wt.EndTime - wt.StartTime <= WorkingLimit).WithMessage(time =>
                    $"You cannot indicate that you worked more than {WorkingLimit.Hours} hours and {WorkingLimit.Minutes} minutes.")
                .Must(wt =>
                {
                    var oldWorkTimes = repository.GetUserWorkTimes(
                        wt.WorkerUserId,
                        new WorkTimeFilter
                        {
                            StartTime = wt.StartTime.AddMinutes(-WorkingLimit.TotalMinutes),
                            EndTime = wt.EndTime
                        });

                    return oldWorkTimes.All(oldWorkTime =>
                        wt.EndTime <= oldWorkTime.StartTime || oldWorkTime.EndTime <= wt.StartTime);
                }).WithMessage("New WorkTime should not overlap with old ones.");
        }
    }
}
