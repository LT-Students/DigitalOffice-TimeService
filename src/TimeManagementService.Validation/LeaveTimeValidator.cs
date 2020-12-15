using FluentValidation;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Broker.Responses;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Exceptions;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LT.DigitalOffice.TimeManagementService.Validation
{
    public class LeaveTimeValidator : AbstractValidator<LeaveTime>
    {
        public LeaveTimeValidator(
            [FromServices] ILeaveTimeRepository repository,
            [FromServices] IAssignValidator assignValidator)
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .DependentRules(() =>
                {
                    RuleFor(x => x)
                    .Must(x => assignValidator.CanAssignUser(x.CurrentUserId, x.UserId))
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
                .Must(x => x.StartTime < x.EndTime).WithMessage("Start time must be before end time.")
                .Must(x =>
                {
                    var workTimes = repository.GetUserLeaveTimes(x.UserId);

                    return workTimes.All(oldWorkTime =>
                        x.EndTime <= oldWorkTime.StartTime || oldWorkTime.EndTime <= x.StartTime);
                }).WithMessage("New LeaveTime should not overlap with old ones.");
        }
    }
}
