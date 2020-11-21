using FluentValidation;
using LT.DigitalOffice.TimeManagementService.Data.Filters;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace LT.DigitalOffice.TimeManagementService.Validation
{
    public class EditWorkTimeRequestValidator : AbstractValidator<(JsonPatchDocument<DbWorkTime>, Guid)>
    {
        /// <summary>
        /// Limit on working hours in a row.
        /// </summary>
        public static TimeSpan WorkingLimit { get; } = new TimeSpan(24, 0, 0);

        public EditWorkTimeRequestValidator([FromServices] IWorkTimeRepository repository)
        {
            RuleFor(wt => wt.Item1.Operations)
                .Must(x => x.Select(x => x.path).Distinct().Count() == x.Count)
                .WithMessage("You don't have to change the same field of WorkTime multiple times");

            RuleFor(wt => wt.Item1.Operations.FirstOrDefault(x => x.path == "/Id"))
                .Null().WithMessage("ID cannot be edited.");

            When(wt => wt.Item1.Operations.FirstOrDefault(x => x.path == "/WorkerUserId") != null, () =>
            {
                RuleFor(wt => (Guid)wt.Item1.Operations.FirstOrDefault(x => x.path == "/WorkerUserId").value)
                .NotEmpty()
                .WithMessage("WorkerUserId must not be empty");
            });

            When(wt => wt.Item1.Operations.FirstOrDefault(x => x.path == "/StartTime") != null, () =>
            {
                RuleFor(wt => (DateTime)wt.Item1.Operations.FirstOrDefault(x => x.path == "/StartTime").value)
                .NotEqual(new DateTime())
                .WithMessage("StartTime must not be empty");
            });

            When(wt => wt.Item1.Operations.FirstOrDefault(x => x.path == "/EndTime") != null, () =>
            {
                RuleFor(wt => (DateTime)wt.Item1.Operations.FirstOrDefault(x => x.path == "/EndTime").value)
                .NotEqual(new DateTime())
                .WithMessage("EndTime must not be empty");
            });

            When(wt => wt.Item1.Operations.FirstOrDefault(x => x.path == "/Title") != null, () =>
            {
                RuleFor(wt => (string)wt.Item1.Operations.FirstOrDefault(x => x.path == "/Title").value)
                .MaximumLength(32)
                .WithMessage("Title is too long.")
                .MinimumLength(2)
                .WithMessage("Title is too short.");
            });

            When(wt => wt.Item1.Operations.FirstOrDefault(x => x.path == "/ProjectId") != null, () =>
            {
                RuleFor(wt => (Guid)wt.Item1.Operations.FirstOrDefault(x => x.path == "/ProjectId").value)
                .NotEmpty()
                .WithMessage("ProjectId must not be empty");
            });

            When(wt => wt.Item1.Operations.FirstOrDefault(x => x.path == "/Description") != null, () =>
            {
                RuleFor(wt => (string)wt.Item1.Operations.FirstOrDefault(x => x.path == "/Description").value)
                .MaximumLength(500)
                .WithMessage("Description is too long.");
            });

            When(wt => wt.Item1.Operations.FirstOrDefault(x => x.path == "/StartTime" || x.path == "/EndTime") != null, () =>
            {
                var userId = new Guid();
                var startTime = new DateTime();
                var endTime = new DateTime();

                RuleFor(wt => wt)
                .Must(wt =>
                {
                    var oldWorkTime = repository.GetWorkTime(wt.Item2);
                    var startTimeOperation = wt.Item1.Operations.FirstOrDefault(x => x.path == "/StartTime");
                    var endTimeOperation = wt.Item1.Operations.FirstOrDefault(x => x.path == "/EndTime");
                    userId = oldWorkTime.WorkerUserId;

                    startTime = startTimeOperation != null ? (DateTime)startTimeOperation.value : oldWorkTime.StartTime;
                    endTime = endTimeOperation != null ? (DateTime)endTimeOperation.value : oldWorkTime.EndTime;

                    return startTime < endTime;
                })
                .WithMessage("You cannot indicate that you worked zero hours or a negative amount.")
                .Must(wt => endTime - startTime <= WorkingLimit).WithMessage(time =>
                    $"You cannot indicate that you worked more than {WorkingLimit.Hours} hours and {WorkingLimit.Minutes} minutes.")
                .Must(wt =>
                {
                    var oldWorkTimes = repository.GetUserWorkTimes(
                        userId,
                        new WorkTimeFilter
                        {
                            EndTime = endTime
                        });

                    if (oldWorkTimes == null) return true;

                    return oldWorkTimes.All(oldWorkTime =>
                        endTime <= oldWorkTime.StartTime || oldWorkTime.EndTime <= startTime);
                }).WithMessage("New WorkTime should not overlap with old ones.");
            });
        }
    }
}
