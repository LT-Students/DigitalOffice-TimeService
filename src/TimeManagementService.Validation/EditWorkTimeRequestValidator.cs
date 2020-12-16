using FluentValidation;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces;
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
            => new List<string> { DescriptionPath, TitlePath, ProjectIdPath, UserIdPath, StartTimePath, EndTimePath };

        public static string DescriptionPath => $"/{nameof(DbWorkTime.Description)}";
        public static string TitlePath => $"/{nameof(DbWorkTime.Title)}";
        public static string ProjectIdPath => $"/{nameof(DbWorkTime.ProjectId)}";
        public static string UserIdPath => $"/{nameof(DbWorkTime.UserId)}";
        public static string StartTimePath => $"/{nameof(DbWorkTime.StartDate)}";
        public static string EndTimePath => $"/{nameof(DbWorkTime.EndDate)}";

        Func<JsonPatchDocument<DbWorkTime>, string, Operation> GetOperationByPath =>
            (x, path) => x.Operations.FirstOrDefault(x => x.path == path);

        /// <summary>
        /// Limit on working hours in a row.
        /// </summary>
        public static TimeSpan WorkingLimit { get; } = new TimeSpan(24, 0, 0);

        public EditWorkTimeRequestValidator(
            [FromServices] IWorkTimeRepository repository,
            [FromServices] IAssignUserValidator assignUserValidator,
            [FromServices] IAssignProjectValidator assignProjectValidator)
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

                    When(wt => GetOperationByPath(wt.Patch, UserIdPath) != null, () =>
                    {
                        RuleFor(x => x.Patch.Operations)
                        .UniqueOperationWithAllowedOp(UserIdPath, "add", "replace");

                        RuleFor(wt => (Guid)GetOperationByPath(wt.Patch, UserIdPath).value)
                        .NotEmpty()
                        .WithMessage($"{UserIdPath} must not be empty.")
                        .DependentRules(() =>
                        {
                            RuleFor(wt => wt)
                            .Must(wt => assignUserValidator.CanAssignUser(wt.CurrentUserId, (Guid)GetOperationByPath(wt.Patch, UserIdPath).value))
                            .WithMessage("You cannot assign this user.");
                        })
                        .WithMessage("User does not exist.");
                    });

                    When(wt => GetOperationByPath(wt.Patch, ProjectIdPath) != null, () =>
                    {
                        RuleFor(x => x.Patch.Operations)
                        .UniqueOperationWithAllowedOp(ProjectIdPath, "add", "replace");

                        RuleFor(wt => (Guid)GetOperationByPath(wt.Patch, ProjectIdPath).value)
                        .NotEmpty()
                        .DependentRules(() =>
                        {
                            When(wt => GetOperationByPath(wt.Patch, UserIdPath) != null, () =>
                            {
                                RuleFor(x => x)
                                .Must(x => assignProjectValidator.CanAssignProject(
                                    (Guid)GetOperationByPath(x.Patch, UserIdPath).value,
                                    (Guid)GetOperationByPath(x.Patch, ProjectIdPath).value))
                                .WithMessage("You cannot assign this user on this project.");
                            })
                            .Otherwise(() =>
                            {
                                RuleFor(x => x)
                                .Must(x => assignProjectValidator.CanAssignProject(
                                    repository.GetWorkTimeById(x.WorkTimeId).UserId,
                                    (Guid)GetOperationByPath(x.Patch, ProjectIdPath).value))
                                .WithMessage("You cannot assign this user on this project.");
                            });
                        })
                        .WithMessage("Project does not exist.");
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
                        RuleFor(wt => wt)
                        .Must(wt =>
                        {
                            var oldWorkTime = repository.GetWorkTimeById(wt.WorkTimeId);
                            var startTimeOperation = GetOperationByPath(wt.Patch, StartTimePath);
                            var endTimeOperation = GetOperationByPath(wt.Patch, EndTimePath);

                            var startTime = startTimeOperation != null ? (DateTime)startTimeOperation.value : oldWorkTime.StartDate;
                            var endTime = endTimeOperation != null ? (DateTime)endTimeOperation.value : oldWorkTime.EndDate;

                            return startTime < endTime;
                        })
                        .WithMessage("The start date must be before the end date.");
                    });
                });
        }
    }
}
