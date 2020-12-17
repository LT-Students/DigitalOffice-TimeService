using FluentValidation;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeManagementService.Validation
{
    public class WorkTimeRequestValidator : AbstractValidator<WorkTimeRequest>
    {
        public WorkTimeRequestValidator(
            [FromServices] IAssignUserValidator assignUserValidator,
            [FromServices] IAssignProjectValidator assignProjectValidator)
        {
            When(x => x.UserId != null, () =>
            {
                RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User does not exist.");
            });

            RuleFor(x => x)
                .Must(x => assignUserValidator.CanAssignUser(x.CurrentUserId, x.UserId ?? x.CurrentUserId))
                .WithMessage("You cannot assign this user.");

            RuleFor(x => x.StartDate)
                .NotEqual(new DateTime());

            RuleFor(x => x.EndDate)
                .NotEqual(new DateTime());

            RuleFor(x => x.Minutes)
                .GreaterThan(0);

            RuleFor(x => x.Title)
                .MinimumLength(2)
                .MaximumLength(128);

            RuleFor(x => x.Description)
                .MaximumLength(1000);

            RuleFor(x => x)
                .Must(x => x.StartDate < x.EndDate)
                .WithMessage("The start date must be before the end date.");

            RuleFor(x => x.ProjectId)
                .NotEmpty()
                .DependentRules(() =>
                {
                    RuleFor(x => x)
                    .Must(x => assignProjectValidator.CanAssignProject(x.UserId ?? x.CurrentUserId, x.ProjectId))
                    .WithMessage("You cannot assign this user on this project.");
                })
                .WithMessage("Project does not exist.");
        }
    }
}
