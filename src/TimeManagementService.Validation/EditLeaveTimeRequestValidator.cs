﻿using FluentValidation;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.TimeManagementService.Data.Filters;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeManagementService.Validation
{
    public class EditLeaveTimeRequestValidator : AbstractValidator<EditLeaveTimeRequest>
    {
        private static List<string> Paths
            => new List<string> { CommentPath, LeaveTypePath, UserIdPath, StartTimePath, EndTimePath };

        private static string CommentPath => $"/{nameof(DbLeaveTime.Comment)}";
        private static string LeaveTypePath => $"/{nameof(DbLeaveTime.LeaveType)}";
        private static string UserIdPath => $"/{nameof(DbLeaveTime.UserId)}";
        private static string StartTimePath => $"/{nameof(DbLeaveTime.StartTime)}";
        private static string EndTimePath => $"/{nameof(DbLeaveTime.EndTime)}";

        Func<JsonPatchDocument<DbLeaveTime>, string, Operation> GetOperationByPath =>
            (x, path) => x.Operations.FirstOrDefault(x => x.path == path);

        public EditLeaveTimeRequestValidator(
            [FromServices] ILeaveTimeRepository repository,
            [FromServices] IAssignValidator assignValidator)
        {
            RuleFor(x => x.Patch.Operations)
                .Must(x => x.Select(x => x.path).Distinct().Count() == x.Count())
                .WithMessage("You don't have to change the same field of LeaveTime multiple times.")
                .Must(x => x.Any())
                .WithMessage("You don't have changes.")
                .ForEach(x => x
                .Must(x => Paths.Contains(x.path))
                .WithMessage($"Document contains invalid path. Only such paths are allowed: {Paths.Aggregate((x, y) => x + ", " + y)}")
                )
                .DependentRules(() =>
                {
                    When(x => GetOperationByPath(x.Patch, StartTimePath) != null, () =>
                    {
                        RuleFor(x => x.Patch.Operations)
                        .UniqueOperationWithAllowedOp(StartTimePath, "add", "replace");

                        RuleFor(x => (DateTime)GetOperationByPath(x.Patch, StartTimePath).value)
                        .NotEqual(new DateTime())
                        .WithMessage($"{StartTimePath} must not be empty.");
                    });

                    When(lt => GetOperationByPath(lt.Patch, EndTimePath) != null, () =>
                    {
                        RuleFor(x => x.Patch.Operations)
                        .UniqueOperationWithAllowedOp(EndTimePath, "add", "replace");

                        RuleFor(x => (DateTime)GetOperationByPath(x.Patch, EndTimePath).value)
                        .NotEqual(new DateTime())
                        .WithMessage($"{EndTimePath} must not be empty.");
                    });

                    When(lt => GetOperationByPath(lt.Patch, LeaveTypePath) != null, () =>
                    {
                        RuleFor(x => x.Patch.Operations)
                        .UniqueOperationWithAllowedOp(LeaveTypePath, "add", "replace");

                        RuleFor(x => (string)GetOperationByPath(x.Patch, LeaveTypePath).value)
                        .MaximumLength(32)
                        .WithMessage($"{LeaveTypePath} is too long.")
                        .MinimumLength(2)
                        .WithMessage($"{LeaveTypePath} is too short.");
                    });

                    // Oh.Check overlap with new User? Admin can it?
                    When(lt => GetOperationByPath(lt.Patch, UserIdPath) != null, () =>
                    {
                        RuleFor(x => x.Patch.Operations)
                        .UniqueOperationWithAllowedOp(UserIdPath, "add", "replace");

                        RuleFor(x => (Guid)GetOperationByPath(x.Patch, UserIdPath).value)
                        .NotEmpty()
                        .WithMessage($"{UserIdPath} must not be empty.")
                        .DependentRules(() =>
                        {
                            RuleFor(x => x)
                            .Must(x => assignValidator.CanAssignUser(x.CurrentUserId, (Guid)GetOperationByPath(x.Patch, UserIdPath).value))
                            .WithMessage("You cannot assign this user.");
                        })
                        .WithMessage("User does not exist.");
                    });

                    When(x => GetOperationByPath(x.Patch, CommentPath) != null, () =>
                    {
                        RuleFor(x => x.Patch.Operations)
                        .UniqueOperationWithAllowedOp(CommentPath, "add", "replace");

                        RuleFor(x => (string)GetOperationByPath(x.Patch, CommentPath).value)
                        .MaximumLength(500)
                        .WithMessage($"{CommentPath} is too long.");
                    });

                    When(x => x.Patch.Operations.FirstOrDefault(x => x.path == StartTimePath || x.path == EndTimePath) != null, () =>
                    {
                        var userId = new Guid();
                        var startTime = new DateTime();
                        var endTime = new DateTime();
                        var oldWorkTimeId = new Guid();

                        RuleFor(x => x)
                        .Must(x =>
                        {
                            var oldWorkTime = repository.GetLeaveTimeById(x.LeaveTimeId);
                            var startTimeOperation = GetOperationByPath(x.Patch, StartTimePath);
                            var endTimeOperation = GetOperationByPath(x.Patch, EndTimePath);
                            userId = oldWorkTime.UserId;
                            oldWorkTimeId = oldWorkTime.Id;

                            startTime = startTimeOperation != null ? (DateTime)startTimeOperation.value : oldWorkTime.StartTime;
                            endTime = endTimeOperation != null ? (DateTime)endTimeOperation.value : oldWorkTime.EndTime;

                            return startTime < endTime;
                        })
                        .WithMessage("You cannot indicate that you worked zero hours or a negative amount.")
                        .Must(x => endTime - startTime <= WorkingLimit)
                        .WithMessage(x => $"You cannot indicate that you worked more than {WorkingLimit.Hours} hours and {WorkingLimit.Minutes} minutes.")
                        .Must(x =>
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
                        })
                        .WithMessage("New WorkTime should not overlap with old ones.");
                    });
                });
        }
    }
}
