using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using FluentValidation;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Resources;

namespace LT.DigitalOffice.TimeService.Validation.LeaveTime
{
  public class CreateLeaveTimeRequestValidator : AbstractValidator<CreateLeaveTimeRequest>, ICreateLeaveTimeRequestValidator
  {
    //dbLeaveTime is always null here, it is used for time validation in editLeaveTimeRequest
    private (DateTimeOffset? startTime, DateTimeOffset? endTime, DbLeaveTime leaveTime, Guid? userId) GetItems(
      DateTimeOffset startTime,
      DateTimeOffset? endTime,
      Guid userId)
    {
      return (startTime: startTime, endTime: endTime, leaveTime: null, userId: userId);
    }

    public CreateLeaveTimeRequestValidator(
      IUserService userService,
      ILeaveTimeIntervalValidator leaveTimeIntervalValidator)
    {
      Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");

      RuleFor(lt => lt.UserId)
        .NotEmpty()
        .MustAsync(async (userId, _) => (await userService.CheckUsersExistenceAsync(new List<Guid> { userId }))?.Count == 1)
        .WithMessage(LeaveTimeValidatorResource.UserDoesNotExist);

      RuleFor(lt => lt.LeaveType)
        .IsInEnum();

      RuleFor(lt => lt.Minutes)
        .GreaterThan(0);

      RuleFor(lt => lt)
        .Must(lt => lt.EndTime.HasValue && lt.LeaveType != LeaveType.Prolonged || !lt.EndTime.HasValue && lt.LeaveType == LeaveType.Prolonged)
        .WithMessage($"{LeaveTimeValidatorResource.IncorrectFormat} {nameof(EditLeaveTimeRequest.StartTime)} or {nameof(EditLeaveTimeRequest.EndTime)}")
        .DependentRules(() =>
        {
          RuleFor(lt => GetItems(lt.StartTime, lt.EndTime, lt.UserId))
            .SetValidator(leaveTimeIntervalValidator);
        });

      RuleFor(lt => lt.Comment)
        .MaximumLength(500).WithMessage($"{nameof(CreateLeaveTimeRequest.Comment)} {LeaveTimeValidatorResource.LongPropertyValue}");

      When(lt => lt.LeaveType == LeaveType.Prolonged, () =>
      {
        RuleFor(lt => lt.Comment)
          .NotEmpty()
          .WithMessage(LeaveTimeValidatorResource.CommentIsEmpty);
      });
    }
  }
}
