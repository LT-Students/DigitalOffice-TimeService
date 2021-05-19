using FluentValidation;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Models.Dto.Requests.HelpersModels;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Validation
{
    public class EditWorkTimeRequestValidator : AbstractValidator<EditWorkTimeModel>, IEditWorkTimeRequestValidator
    {
        /// <summary>
        /// Limit on working hours in a row.
        /// </summary>
        public static TimeSpan WorkingLimit { get; } = new TimeSpan(24, 0, 0);

        public static List<string> Paths =>
            new List<string>
            {
                ProjectId,
                StartTime,
                EndTime,
                Title,
                Description
            };

        public static string ProjectId => $"/{nameof(EditWorkTimeRequest.ProjectId)}";
        public static string StartTime => $"/{nameof(EditWorkTimeRequest.StartTime)}";
        public static string EndTime => $"/{nameof(EditWorkTimeRequest.EndTime)}";
        public static string Title => $"/{nameof(EditWorkTimeRequest.Title)}";
        public static string Description => $"/{nameof(EditWorkTimeRequest.Description)}";

        Func<EditWorkTimeModel, string, Operation> GetOperationByPath =>
            (x, path) =>
                x.JsonPatchDocument.Operations.FirstOrDefault(x =>
                    string.Equals(x.path, path, StringComparison.OrdinalIgnoreCase));

        public EditWorkTimeRequestValidator(IWorkTimeRepository repository)
        {
            RuleFor(x => x.JsonPatchDocument.Operations)
                .Must(x => x.Select(x => x.path).Distinct().Count() == x.Count())
                .WithMessage("You don't have to change the same field of Project multiple times.")
                .Must(x => x.Any())
                .WithMessage("You don't have changes.")
                .ForEach(y => y
                    .Must(x => Paths.Any(cur => string.Equals(cur, x.path, StringComparison.OrdinalIgnoreCase)))
                    .WithMessage(
                        $"Document contains invalid path. Only such paths are allowed: {Paths.Aggregate((x, y) => x + ", " + y)}"))
                .DependentRules(() =>
                {
                    When(x => GetOperationByPath(x, ProjectId) != null, () =>
                    {
                        RuleFor(x => x.JsonPatchDocument.Operations).UniqueOperationWithAllowedOp(ProjectId, "replace");

                        RuleFor(x => GetOperationByPath(x, ProjectId).value)
                            .Must(x => Guid.TryParse(x.ToString(), out Guid _))
                            .WithMessage("Wrong ProjectId value.")
                            .NotEmpty();
                    });

                    When(x => GetOperationByPath(x, Title) != null, () =>
                    {
                        RuleFor(x => x.JsonPatchDocument.Operations).UniqueOperationWithAllowedOp(Title, "replace");

                        RuleFor(x => (string)GetOperationByPath(x, Title).value)
                            .NotEmpty()
                            .MaximumLength(200)
                            .WithMessage("Title is too long.");
                    });

                    When(x => GetOperationByPath(x, Description) != null, () =>
                    {
                        RuleFor(x => x.JsonPatchDocument.Operations).UniqueOperationWithAllowedOp(Description, "replace");
                    });

                    When(x => GetOperationByPath(x, StartTime) != null, () =>
                    {
                        RuleFor(x => x.JsonPatchDocument.Operations).UniqueOperationWithAllowedOp(StartTime, "replace");

                        RuleFor(x => (DateTime)GetOperationByPath(x, StartTime).value)
                            .NotEqual(new DateTime())
                            .WithMessage("StartTime must not be empty.");
                    });

                    When(x => GetOperationByPath(x, EndTime) != null, () =>
                    {
                        RuleFor(x => x.JsonPatchDocument.Operations).UniqueOperationWithAllowedOp(EndTime, "replace");

                        RuleFor(x => (DateTime)GetOperationByPath(x, EndTime).value)
                            .NotEqual(new DateTime())
                            .WithMessage("EndTime must not be empty.");
                    });

                    When(x => GetOperationByPath(x, StartTime) != null && GetOperationByPath(x, EndTime) != null, () =>
                    {
                        RuleFor(x => x.JsonPatchDocument.Operations).UniqueOperationWithAllowedOp(StartTime, "replace");
                        RuleFor(x => x.JsonPatchDocument.Operations).UniqueOperationWithAllowedOp(EndTime, "replace");

                        RuleFor(x => (DateTime)GetOperationByPath(x, EndTime).value)
                            .NotEqual(new DateTime())
                            .WithMessage("EndTime must not be empty.");

                        RuleFor(x => x)
                            .Must(x => 
                                (DateTime)GetOperationByPath(x, StartTime).value < (DateTime)GetOperationByPath(x, EndTime).value)
                            .WithMessage("You cannot indicate that you worked zero hours or a negative amount.")
                            .Must(x =>
                                (DateTime)GetOperationByPath(x, EndTime).value - (DateTime)GetOperationByPath(x, StartTime).value <= WorkingLimit)
                            .WithMessage(time =>
                                $"You cannot indicate that you worked more than {WorkingLimit.Hours} hours and {WorkingLimit.Minutes} minutes.")
                            .Must(x =>
                            {
                                var oldWorkTimes = repository.Find(
                                    new FindWorkTimesFilter
                                    {
                                        UserId = x.UserId,
                                        EndTime = (DateTime)GetOperationByPath(x, EndTime).value
                                    },
                                    0,
                                    int.MaxValue,
                                    out _);

                                if (oldWorkTimes == null)
                                {
                                    return true;
                                }

                                return oldWorkTimes
                                    .Where(oldWorkTime => oldWorkTime.Id != x.Id)
                                    .All(oldWorkTime =>
                                        (DateTime)GetOperationByPath(x, EndTime).value <= oldWorkTime.StartTime
                                        || oldWorkTime.EndTime <= (DateTime)GetOperationByPath(x, StartTime).value);
                            }).WithMessage("New work time should not overlap with old ones.");
                    });
                });
        }
    }
}
