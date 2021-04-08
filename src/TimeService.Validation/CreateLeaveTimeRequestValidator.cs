using FluentValidation;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Validation
{
    public class CreateLeaveTimeRequestValidator : AbstractValidator<CreateLeaveTimeRequest>, ICreateLeaveTimeRequestValidator
    {
        public CreateLeaveTimeRequestValidator(ILeaveTimeRepository repository)
        {
            RuleFor(lt => lt.WorkerUserId)
                .NotEmpty();

            RuleFor(lt => lt.LeaveType)
                .IsInEnum();

            RuleFor(lt => lt.Comment)
                .NotEmpty();

            RuleFor(lt => lt.StartTime)
                .NotEqual(new DateTime());

            RuleFor(lt => lt.EndTime)
                .NotEqual(new DateTime());

            RuleFor(lt => lt)
                .Must(lt => lt.StartTime < lt.EndTime).WithMessage("Start time must be before end time")
                .Must(lt =>
                {
                    var workTimes = repository.GetUserLeaveTimes(lt.WorkerUserId);

                    return workTimes.All(oldWorkTime =>
                        lt.EndTime <= oldWorkTime.StartTime || oldWorkTime.EndTime <= lt.StartTime);
                }).WithMessage("New LeaveTime should not overlap with old ones.");
        }
    }
}
