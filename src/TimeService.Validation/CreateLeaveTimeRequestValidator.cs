using FluentValidation;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using System;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Validation
{
    public class CreateLeaveTimeRequestValidator : AbstractValidator<CreateLeaveTimeRequest>, ICreateLeaveTimeRequestValidator
    {
        public CreateLeaveTimeRequestValidator(ILeaveTimeRepository repository)
        {
            RuleFor(lt => lt.UserId)
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
                .Must(lt => lt.StartTime < lt.EndTime).WithMessage("Start time must be before end time.")
                .Must(lt =>
                {
                    var leaveTimes = repository.Find(new FindLeaveTimesFilter { UserId = lt.UserId }, 0, int.MaxValue, out _);

                    return leaveTimes.All(oldLeaveTime =>
                        lt.EndTime <= oldLeaveTime.StartTime || oldLeaveTime.EndTime <= lt.StartTime);
                }).WithMessage("New LeaveTime should not overlap with old ones.");
        }
    }
}
