using FluentValidation;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Validation
{
    public class EditWorkTimeRequestValidator : AbstractValidator<EditWorkTimeRequest>, IEditWorkTimeRequestValidator
    {
        /// <summary>
        /// Limit on working hours in a row.
        /// </summary>
        public static TimeSpan WorkingLimit { get; } = new TimeSpan(24, 0, 0);

        public EditWorkTimeRequestValidator(IWorkTimeRepository repository)
        {
            RuleFor(wt => wt.Id)
                .NotEmpty()
                .WithMessage("WorkTimeId must not be empty");

            RuleFor(wt => wt.WorkerUserId)
                .NotEmpty()
                .WithMessage("WorkerUserId must not be empty");

            RuleFor(wt => wt.StartTime)
                .NotEqual(new DateTime())
                .WithMessage("StartTime must not be empty");

            RuleFor(wt => wt.EndTime)
                .NotEqual(new DateTime())
                .WithMessage("EndTime must not be empty");

            RuleFor(wt => wt.Title)
                .MaximumLength(32)
                .WithMessage("Title is too long.")
                .MinimumLength(2)
                .WithMessage("Title is too short.");

            RuleFor(wt => wt.ProjectId)
                .NotEmpty()
                .WithMessage("ProjectId must not be empty");

            RuleFor(wt => wt.Description)
                .MaximumLength(500)
                .WithMessage("Description is too long.");

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
                            EndTime = wt.EndTime
                        });

                    if (oldWorkTimes == null) return true;

                    return oldWorkTimes.All(oldWorkTime =>
                        wt.EndTime <= oldWorkTime.StartTime || oldWorkTime.EndTime <= wt.StartTime);
                }).WithMessage("New WorkTime should not overlap with old ones.");
        }
    }
}
