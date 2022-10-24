using System;
using System.Globalization;
using System.Threading;
using FluentValidation;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Resources;

namespace LT.DigitalOffice.TimeService.Validation.LeaveTime
{
  public class LeaveTimeIntervalValidator : AbstractValidator<(DateTimeOffset? startTime, DateTimeOffset? endTime, DbLeaveTime leaveTime, Guid? userId)>, ILeaveTimeIntervalValidator
  {
    private bool ValidateInterval(DateTimeOffset? startTime, DateTimeOffset? endTime, DbLeaveTime dbLeaveTime)
    {
      if (startTime is null && endTime is null)
      {
        return true;
      }

      DateTime userTimeNow = DateTime.UtcNow.Add(startTime?.Offset ?? endTime.Value.Offset);

      DateTime thisMonthFirstDay = new DateTime(userTimeNow.Year, userTimeNow.Month, 1);

      DateTime? startMonthFirstDay = startTime is null
        ? null
        : new DateTime(startTime.Value.Year, startTime.Value.Month, 1);

      DateTime? endMonthFirstDay = endTime is null
        ? null
        : new DateTime(endTime.Value.Year, endTime.Value.Month, 1);

      //if start time and end time are edited or created, they and old times from db must be in previous (if today is the 5th or less day), current or next month
      bool isStartTimeValid = startTime is null  //startTime is valid if it is not edited
        || (dbLeaveTime is null || dbLeaveTime.StartTime.Add(startTime.Value.Offset) >= thisMonthFirstDay  //checks that start time was in correct interval if leaveTime is edited
          || (dbLeaveTime.StartTime.Add(startTime.Value.Offset) >= thisMonthFirstDay.AddMonths(-1) && userTimeNow.Day <= 5))
        && ((startMonthFirstDay == thisMonthFirstDay.AddMonths(-1) && userTimeNow.Day <= 5)  //checks that new start time is in correct interval
          || startMonthFirstDay == thisMonthFirstDay || startMonthFirstDay == thisMonthFirstDay.AddMonths(1));

      bool isEndTimeValid = endTime is null  //endTime is valid if it is not edited
        || (dbLeaveTime is null || dbLeaveTime.EndTime.Add(endTime.Value.Offset) >= thisMonthFirstDay  //checks that end time was in correct interval if leaveTime is edited
          || (dbLeaveTime.EndTime.Add(endTime.Value.Offset) >= thisMonthFirstDay.AddMonths(-1) && userTimeNow.Day <= 5))
        && ((endMonthFirstDay == thisMonthFirstDay.AddMonths(-1) && userTimeNow.Day <= 5)  //checks that new end time is in correct interval
          || endMonthFirstDay == thisMonthFirstDay || endMonthFirstDay == thisMonthFirstDay.AddMonths(1));

      return isStartTimeValid && isEndTimeValid;
    }

    public LeaveTimeIntervalValidator(ILeaveTimeRepository repository)
    {
      Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");

      CascadeMode = CascadeMode.Stop;

      RuleFor(x => x)
        .Must(x => ValidateInterval(x.startTime, x.endTime, x.leaveTime)).WithMessage(LeaveTimeValidatorResource.LeaveTimeIntervalIsNotCorrect)
        .MustAsync(async (x, _) =>
          (x.leaveTime is not null
            && !await repository.HasOverlapAsync(
              leaveTime: x.leaveTime,
              start: x.startTime?.UtcDateTime ?? x.leaveTime.StartTime,
              end: x.endTime?.UtcDateTime ?? x.leaveTime.EndTime))
          || x.userId.HasValue && x.startTime.HasValue
            && !await repository.HasOverlapAsync(
              userId: x.userId.Value,
              start: x.startTime.Value.UtcDateTime,
              end: x.endTime?.UtcDateTime))
        .WithMessage(LeaveTimeValidatorResource.LeaveTimesOverlap);

      When(x => x.startTime.HasValue && x.endTime.HasValue,
        () =>
        {
          RuleFor(x => x)
            .Must(x => x.startTime.Value.Offset.Equals(x.endTime.Value.Offset)).WithMessage(LeaveTimeValidatorResource.OffsetsAreNotSame);
        });

      When(x => x.endTime.HasValue || x.leaveTime is not null,
        () =>
        {
          RuleFor(x => x)
            .Must(x => (x.startTime?.UtcDateTime ?? x.leaveTime?.StartTime) <= (x.endTime?.UtcDateTime ?? x.leaveTime?.EndTime))
            .WithMessage(LeaveTimeValidatorResource.StartTimeAfterEndTime);
        });
    }
  }
}
