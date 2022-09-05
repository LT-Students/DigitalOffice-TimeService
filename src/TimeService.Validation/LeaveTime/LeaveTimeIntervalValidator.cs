using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;

namespace LT.DigitalOffice.TimeService.Validation.LeaveTime
{
  public class LeaveTimeIntervalValidator : AbstractValidator<(DateTimeOffset startTime, DateTimeOffset endTime, DbLeaveTime leaveTime, Guid? userId)>, ILeaveTimeIntervalValidator
  {
    private bool ValidateInterval(DateTimeOffset startTime, DateTimeOffset endTime, DbLeaveTime dbLeaveTime)
    {
      DateTime userTimeNow = DateTime.UtcNow.Add(startTime.Offset);

      DateTime thisMonthFirstDay = new DateTime(userTimeNow.Year, userTimeNow.Month, 1);
      DateTime startMonthFirstDay = new DateTime(startTime.Year, startTime.Month, 1);
      DateTime endMonthFirstDay = new DateTime(endTime.Year, endTime.Month, 1);

      //start time and end time must be in previous (if today is the 5th or less day), current or next month
      bool isStartTimeValid = (dbLeaveTime is null || dbLeaveTime.StartTime.Add(startTime.Offset) >= thisMonthFirstDay 
          || (dbLeaveTime.StartTime.Add(startTime.Offset) >= thisMonthFirstDay.AddMonths(-1) && userTimeNow.Day <= 5)) //checks that start time was in correct interval if leaveTime is edited
        && ((startMonthFirstDay == thisMonthFirstDay.AddMonths(-1) && userTimeNow.Day <= 5)
          || startMonthFirstDay == thisMonthFirstDay || startMonthFirstDay == thisMonthFirstDay.AddMonths(1));

      bool isEndTimeValid = (dbLeaveTime is null || dbLeaveTime.EndTime.Add(startTime.Offset) >= thisMonthFirstDay
          || (dbLeaveTime.EndTime.Add(startTime.Offset) >= thisMonthFirstDay.AddMonths(-1) && userTimeNow.Day <= 5)) //checks that end time was in correct interval if leaveTime is edited
        && (endMonthFirstDay == thisMonthFirstDay.AddMonths(-1) && userTimeNow.Day <= 5)
          || endMonthFirstDay == thisMonthFirstDay || endMonthFirstDay == thisMonthFirstDay.AddMonths(1);

      return isStartTimeValid && isEndTimeValid;
    }

    public LeaveTimeIntervalValidator(ILeaveTimeRepository repository)
    {
      CascadeMode = CascadeMode.Stop;

      RuleFor(x => x)
        .Must(x => x.startTime.Offset.Equals(x.endTime.Offset))
        .WithMessage("Start time and end time offsets must be same.")
        .Must(x => x.startTime <= x.endTime).WithMessage("Start time must be before end time.")
        .Must(x => ValidateInterval(x.startTime, x.endTime, x.leaveTime))
        .WithMessage("Incorrect interval for leave time.")
        .MustAsync(async (x, _) =>
          (x.leaveTime is not null && !await repository.HasOverlapAsync(x.leaveTime, x.startTime.UtcDateTime, x.endTime.UtcDateTime))
          || (x.userId.HasValue && !await repository.HasOverlapAsync(x.userId.Value, x.startTime.UtcDateTime, x.endTime.UtcDateTime)))
        .WithMessage("New LeaveTime should not overlap with old ones.");
    }
  }
}
