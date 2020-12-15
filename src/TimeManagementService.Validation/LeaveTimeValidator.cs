using FluentValidation;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace LT.DigitalOffice.TimeManagementService.Validation
{
    public class LeaveTimeValidator : AbstractValidator<LeaveTime>
    {
        public LeaveTimeValidator(
            [FromServices] IAssignUserValidator assignUserValidator)
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

            RuleFor(x => x.LeaveType)
                .IsInEnum();

            RuleFor(x => x.Comment)
                .NotEmpty();

            RuleFor(x => x.StartTime)
                .NotEqual(new DateTime());

            RuleFor(x => x.EndTime)
                .NotEqual(new DateTime());

            RuleFor(x => x)
                .Must(x => x.StartTime < x.EndTime).WithMessage("Start time must be before end time.");
        }
    }
}
