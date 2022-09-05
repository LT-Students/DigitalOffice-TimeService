using System;
using System.Collections.Generic;
using FluentValidation;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Validation.LeaveTime
{
  public class CreateLeaveTimeRequestValidator : AbstractValidator<CreateLeaveTimeRequest>, ICreateLeaveTimeRequestValidator
  {
    private readonly IUserService _userService;

    //dbLeaveTime is always null here, it is used for time validation in editLeaveTimeRequest
    private (DateTimeOffset startTime, DateTimeOffset endTime, DbLeaveTime leaveTime, Guid? userId) GetItems(
      DateTimeOffset startTime,
      DateTimeOffset endTime,
      Guid userId)
    {
      return (startTime: startTime, endTime: endTime, leaveTime: null, userId: userId);
    }

    public CreateLeaveTimeRequestValidator(
      IUserService userService,
      ILeaveTimeIntervalValidator leaveTimeIntervalValidator)
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

      RuleFor(lt => GetItems(lt.StartTime, lt.EndTime, lt.UserId))
        .SetValidator(leaveTimeIntervalValidator);

      RuleFor(lt => lt.Comment)
        .MaximumLength(500).WithMessage("Comment is too long.");
    }
  }
}
