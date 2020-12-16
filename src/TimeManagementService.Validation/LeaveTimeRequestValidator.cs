using FluentValidation;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeManagementService.Validation
{
    public class LeaveTimeRequestValidator : AbstractValidator<LeaveTimeRequest>
    {
        public LeaveTimeRequestValidator(
            [FromServices] IAssignUserValidator assignUserValidator)
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

            RuleFor(x => x.LeaveType)
                .IsInEnum();

            RuleFor(x => x.Comment)
                .MaximumLength(10000);

            RuleFor(x => x.StartTime)
                .NotEqual(new DateTime());

            RuleFor(x => x.EndTime)
                .NotEqual(new DateTime());

            RuleFor(x => x)
                .Must(x => x.StartTime < x.EndTime).WithMessage("The start date must be before the end date.");
        }
    }
}
