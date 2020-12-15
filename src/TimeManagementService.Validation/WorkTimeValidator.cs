using FluentValidation;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeManagementService.Validation
{
    public class WorkTimeValidator : AbstractValidator<WorkTime>
    {
        public WorkTimeValidator(
            [FromServices] IAssignUserValidator assignUserValidator,
            [FromServices] IAssignProjectValidator assignProjectValidator)
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .DependentRules(() =>
                {
                    RuleFor(x => x)
                    .Must(x => assignUserValidator.CanAssignUser(x.CurrentUserId, x.UserId ?? x.CurrentUserId))
                    .WithMessage("You cannot assign this user.");
                })
                .WithMessage("User does not exist.");

            RuleFor(x => x.StartDate)
                .NotEqual(new DateTime());

            RuleFor(x => x.EndDate)
                .NotEqual(new DateTime());

            RuleFor(x => x.ProjectId)
                .NotEmpty()
                .DependentRules(() =>
                {
                    RuleFor(x => x)
                    .Must(x => assignProjectValidator.CanAssignProject(x.UserId ?? x.CurrentUserId, x.ProjectId))
                    .WithMessage("You cannot assign this user on this project.");
                })
                .WithMessage("Project does not exist."); ;

            RuleFor(x => x.Title)
                .NotEmpty();

            RuleFor(x => x)
                .Must(x => x.StartDate < x.EndDate)
                .WithMessage("You cannot indicate that you worked zero hours or a negative amount.");
        }
    }
}
