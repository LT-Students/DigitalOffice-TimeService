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
    private readonly ILogger<CreateLeaveTimeRequestValidator> _logger;

    private bool CheckLeaveTimeInterval(CreateLeaveTimeRequest lt)
    {
      DateTime timeNow = DateTime.UtcNow.Add(lt.StartTime.Offset);

      switch (lt.LeaveType)
      {
        case LeaveType.SickLeave:
          if (lt.StartTime < timeNow.AddMonths(-1) || lt.EndTime > timeNow.AddMonths(1))
          {
            return false;
          }
          break;

        default:
          if (lt.StartTime < timeNow.AddMonths(-1) || (lt.StartTime.Month == timeNow.AddMonths(-1).Month && timeNow.Day > 5))
          {
            return false;
          }
          break;
      }

      return true;
    }

    public CreateLeaveTimeRequestValidator(
      ILeaveTimeRepository repository,
      IUserService userService,
      ILogger<CreateLeaveTimeRequestValidator> logger)
    {
      _userService = userService;
      _logger = logger;

      RuleFor(lt => lt.UserId)
        .NotEmpty()
        .MustAsync(async (userId, cancellation) => (await _userService.CheckUsersExistenceAsync(new List<Guid>() { userId }))?.Count == 1)
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
        .Must(lt => CheckLeaveTimeInterval(lt))
        .WithMessage("Incorrect interval for leave time.")
        .MustAsync(async (lt, _) => !await repository.HasOverlapAsync(lt.UserId, lt.StartTime.UtcDateTime, lt.EndTime.UtcDateTime))
        .WithMessage("New LeaveTime should not overlap with old ones.");

      RuleFor(lt => lt.Comment)
        .MaximumLength(500).WithMessage("Comment is too long.");
    }
  }
}
