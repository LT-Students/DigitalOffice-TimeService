using FluentValidation;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.TimeManagementService.Data.Filters;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeManagementService.Validation
{
    public class EditWorkTimeRequestValidator : AbstractValidator<EditWorkTimeRequest>
    {
        private static List<string> Paths
            => new List<string> { DescriptionPath, TitlePath, ProjectIdPath,// WorkerUserIdPath,
                StartTimePath, EndTimePath };

        private static string DescriptionPath => $"/{nameof(DbWorkTime.Description)}";
        private static string TitlePath => $"/{nameof(DbWorkTime.Title)}";
        private static string ProjectIdPath => $"/{nameof(DbWorkTime.ProjectId)}";
        //private static string WorkerUserIdPath => $"/{nameof(DbWorkTime.WorkerUserId)}";
        private static string StartTimePath => $"/{nameof(DbWorkTime.StartTime)}";
        private static string EndTimePath => $"/{nameof(DbWorkTime.EndTime)}";

        Func<JsonPatchDocument<DbWorkTime>, string, Operation> GetOperationByPath =>
            (x, path) => x.Operations.FirstOrDefault(x => x.path == path);

        /// <summary>
        /// Limit on working hours in a row.
        /// </summary>
        public static TimeSpan WorkingLimit { get; } = new TimeSpan(24, 0, 0);

        public EditWorkTimeRequestValidator([FromServices] IWorkTimeRepository repository)
        {
            RuleFor(x => x.Patch.Operations)
                .Must(x => x.Select(x => x.path).Distinct().Count() == x.Count())
                .WithMessage("You don't have to change the same field of WorkTime multiple times.")
                .Must(x => x.Any())
                .WithMessage("You don't have changes.")
                .ForEach(x => x
                .Must(x => Paths.Contains(x.path))
                .WithMessage($"Document contains invalid path. Only such paths are allowed: {Paths.Aggregate((x, y) => x + ", " + y)}")
                )
                .DependentRules(() =>
                {
                    When(wt => GetOperationByPath(wt.Patch, StartTimePath) != null, () =>
                    {
                        RuleFor(x => x.Patch.Operations)
                        .UniqueOperationWithAllowedOp(StartTimePath, "add", "replace");

                        RuleFor(wt => (DateTime)GetOperationByPath(wt.Patch, StartTimePath).value)
                        .NotEqual(new DateTime())
                        .WithMessage($"{StartTimePath} must not be empty.");
                    });

                    When(wt => GetOperationByPath(wt.Patch, EndTimePath) != null, () =>
                    {
                        RuleFor(x => x.Patch.Operations)
                        .UniqueOperationWithAllowedOp(EndTimePath, "add", "replace");

                        RuleFor(wt => (DateTime)GetOperationByPath(wt.Patch, EndTimePath).value)
                        .NotEqual(new DateTime())
                        .WithMessage($"{EndTimePath} must not be empty.");
                    });

                    When(wt => GetOperationByPath(wt.Patch, TitlePath) != null, () =>
                    {
                        RuleFor(x => x.Patch.Operations)
                        .UniqueOperationWithAllowedOp(TitlePath, "add", "replace");

                        RuleFor(wt => (string)GetOperationByPath(wt.Patch, TitlePath).value)
                        .MaximumLength(32)
                        .WithMessage($"{TitlePath} is too long.")
                        .MinimumLength(2)
                        .WithMessage($"{TitlePath} is too short.");
                    });

                    // Oh. Check overlap with new User? Admin can it?
                    //When(wt => GetOperationByPath(wt.Patch, WorkerUserIdPath) != null, () =>
                    //{
                    //    RuleFor(x => x.Patch.Operations)
                    //    .UniqueOperationWithAllowedOp(WorkerUserIdPath, "add", "replace");

                    //    //check exist?

                    //    RuleFor(wt => (Guid)GetOperationByPath(wt.Patch, WorkerUserIdPath).value)
                    //    .NotEmpty()
                    //    .WithMessage($"{WorkerUserIdPath} must not be empty.");
                    //});

                    When(wt => GetOperationByPath(wt.Patch, ProjectIdPath) != null, () =>
                    {
                        RuleFor(x => x.Patch.Operations)
                        .UniqueOperationWithAllowedOp(ProjectIdPath, "add", "replace");

                        //check exist?
                        //check user projects?

                        RuleFor(wt => (Guid)GetOperationByPath(wt.Patch, ProjectIdPath).value)
                        .NotEmpty()
                        .WithMessage($"{ProjectIdPath} must not be empty.");
                    });

                    When(wt => GetOperationByPath(wt.Patch, DescriptionPath) != null, () =>
                    {
                        RuleFor(x => x.Patch.Operations)
                        .UniqueOperationWithAllowedOp(DescriptionPath, "add", "replace");

                        RuleFor(wt => (string)GetOperationByPath(wt.Patch, DescriptionPath).value)
                        .MaximumLength(500)
                        .WithMessage($"{DescriptionPath} is too long.");
                    });

                    When(wt => wt.Patch.Operations.FirstOrDefault(x => x.path == StartTimePath || x.path == EndTimePath) != null, () =>
                    {
                        var userId = new Guid();
                        var startTime = new DateTime();
                        var endTime = new DateTime();
                        var oldWorkTimeId = new Guid();

                        RuleFor(wt => wt)
                        .Must(wt =>
                        {
                            var oldWorkTime = repository.GetWorkTime(wt.WorkTimeId);
                            var startTimeOperation = GetOperationByPath(wt.Patch, StartTimePath);
                            var endTimeOperation = GetOperationByPath(wt.Patch, EndTimePath);
                            userId = oldWorkTime.WorkerUserId;
                            oldWorkTimeId = oldWorkTime.Id;

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

                            return oldWorkTimes
                            .Where(oldWorkTime => oldWorkTime.Id != oldWorkTimeId)
                            .All(oldWorkTime => endTime <= oldWorkTime.StartTime || oldWorkTime.EndTime <= startTime);
                        }).WithMessage("New WorkTime should not overlap with old ones.");
                    });
                });
        }
    }
}
