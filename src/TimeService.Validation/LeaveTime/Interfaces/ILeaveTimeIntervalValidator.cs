using System;
using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;

namespace LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces
{
  [AutoInject]
  public interface ILeaveTimeIntervalValidator : IValidator<(DateTimeOffset startTime, DateTimeOffset endTime, DbLeaveTime leaveTime, Guid? userId)>
  {
  }
}
