using System;
using System.Collections.Generic;
using FluentValidation;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Validation.LeaveTime
{
  public class CreateLeaveTimeRequestValidator : AbstractValidator<CreateLeaveTimeRequest>, ICreateLeaveTimeRequestValidator
  {
    private readonly IUserService _userService;

    private bool CheckLeaveTimeInterval(CreateLeaveTimeRequest lt)
    {
      DateTime timeNow = DateTime.UtcNow.Add(lt.StartTime.Offset);
      DateTime thisMonthFirstDay = new DateTime(timeNow.Year, timeNow.Month, 1);

      if (lt.EndTime >= thisMonthFirstDay.AddMonths(2)
        || (lt.StartTime < thisMonthFirstDay && timeNow.Day > 5)
        || lt.StartTime < thisMonthFirstDay.AddMonths(-1))
      {
        return false;
      }

      return true;
    }

    public CreateLeaveTimeRequestValidator(
      ILeaveTimeRepository repository,
      IUserService userService)
    {
      _userService = userService;

      RuleFor(lt => lt.UserId)
        .NotEmpty()
        .MustAsync(async (userId, _) => (await _userService.CheckUsersExistenceAsync(new List<Guid> { userId }))?.Count == 1)
        .WithMessage("This user doesn't exist.");

      RuleFor(lt => lt.LeaveType)
        .IsInEnum();

      RuleFor(lt => lt.StartTime)
        .NotEqual(new DateTimeOffset());

      RuleFor(lt => lt.EndTime)
        .NotEqual(new DateTimeOffset());

      RuleFor(lt => lt.Minutes)
        .GreaterThan(0);

      RuleFor(lt => lt)
        .Cascade(CascadeMode.Stop)
        .Must(lt => lt.StartTime.Offset.Equals(lt.EndTime.Offset))
        .WithMessage("Start time and end time offsets must be same.")
        .Must(lt => lt.StartTime <= lt.EndTime)
        .WithMessage("Start time must be before end time.")
        .Must(CheckLeaveTimeInterval)
        .WithMessage("Incorrect interval for leave time.")
        .MustAsync(async (lt, _) => !await repository.HasOverlapAsync(lt.UserId, lt.StartTime.UtcDateTime, lt.EndTime.UtcDateTime))
        .WithMessage("New LeaveTime should not overlap with old ones.");

      RuleFor(lt => lt.Comment)
        .MaximumLength(500).WithMessage("Comment is too long.");
    }
  }
}
