using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using LT.DigitalOffice.Kernel.Validators;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.TimeService.Validation.LeaveTime
{
  public class EditLeaveTimeRequestValidator : ExtendedEditRequestValidator<DbLeaveTime, EditLeaveTimeRequest>, IEditLeaveTimeRequestValidator
  {
    private readonly ILeaveTimeRepository _repository;

    private async Task ValidateOverlappingAsync(
      DbLeaveTime oldLeaveTime,
      List<Operation<EditLeaveTimeRequest>> operations,
      CustomContext context)
    {
      Operation<EditLeaveTimeRequest> startTimeOperation = operations.FirstOrDefault(
        o => o.path.EndsWith(nameof(EditLeaveTimeRequest.StartTime), StringComparison.OrdinalIgnoreCase));
      Operation<EditLeaveTimeRequest> endTimeOperation = operations.FirstOrDefault(
        o => o.path.EndsWith(nameof(EditLeaveTimeRequest.EndTime), StringComparison.OrdinalIgnoreCase));

      if (startTimeOperation is null && endTimeOperation is null)
      {
        return;
      }

      if (!(startTimeOperation is null || DateTimeOffset.TryParse(startTimeOperation.value.ToString(), out _))
        || !(endTimeOperation is null || DateTimeOffset.TryParse(endTimeOperation.value.ToString(), out _)))
      {
        context.AddFailure("Incorrect format of startTime or endTime");
      }

      DateTimeOffset startTimeWithOffset;
      DateTimeOffset endTimeWithOffset;

      if (startTimeOperation is null)
      {
        endTimeWithOffset = DateTimeOffset.Parse(endTimeOperation.value.ToString());
        startTimeWithOffset = new DateTimeOffset(DateTime.SpecifyKind(oldLeaveTime.StartTime, DateTimeKind.Unspecified), endTimeWithOffset.Offset);
      }
      else
      {
        startTimeWithOffset = DateTimeOffset.Parse(startTimeOperation.value.ToString());
        endTimeWithOffset = endTimeOperation is null
          ? new DateTimeOffset(DateTime.SpecifyKind(oldLeaveTime.EndTime, DateTimeKind.Unspecified), startTimeWithOffset.Offset)
          : DateTimeOffset.Parse(endTimeOperation.value.ToString());
      }

      //converting utc time with offset to local time
      DateTime startTime = startTimeWithOffset.DateTime.Add(startTimeWithOffset.Offset);
      DateTime endTime = endTimeWithOffset.DateTime.Add(endTimeWithOffset.Offset);

      if (startTime > endTime)
      {
        context.AddFailure("Start time must be less than end time.");
      }

      DateTime timeNow = DateTime.UtcNow.Add(startTimeWithOffset.Offset);

      DateTime thisMonthFirstDay = new DateTime(timeNow.Year, timeNow.Month, 1);
      DateTime startMonthFirstDay = new DateTime(startTime.Year, startTime.Month, 1);
      DateTime endMonthFirstDay = new DateTime(endTime.Year, endTime.Month, 1);

      bool isEditingStartTimeValid = startTimeOperation is null || (startMonthFirstDay == thisMonthFirstDay.AddMonths(-1) && timeNow.Day <= 5)
        || startMonthFirstDay == thisMonthFirstDay || startMonthFirstDay == thisMonthFirstDay.AddMonths(1);

      bool isEditingEndTimeValid = endTimeOperation is null || (endMonthFirstDay == thisMonthFirstDay.AddMonths(-1) && timeNow.Day <= 5)
        || endMonthFirstDay == thisMonthFirstDay || endMonthFirstDay == thisMonthFirstDay.AddMonths(1);

      if (!isEditingStartTimeValid || !isEditingEndTimeValid)
      {
        context.AddFailure("Incorrect interval for leave time.");
      }

      if (await _repository.HasOverlapAsync(oldLeaveTime, startTimeWithOffset.UtcDateTime, endTimeWithOffset.UtcDateTime))
      {
        context.AddFailure("New LeaveTime should not overlap with old ones.");
      }
    }

    private void HandleInternalPropertyValidation(Operation<EditLeaveTimeRequest> requestedOperation, CustomContext context)
    {
      Context = context;
      RequestedOperation = requestedOperation;

      #region Paths

      AddСorrectPaths(
        new()
        {
          nameof(EditLeaveTimeRequest.StartTime),
          nameof(EditLeaveTimeRequest.EndTime),
          nameof(EditLeaveTimeRequest.Minutes),
          nameof(EditLeaveTimeRequest.LeaveType),
          nameof(EditLeaveTimeRequest.IsActive),
          nameof(EditLeaveTimeRequest.Comment)
        });

      AddСorrectOperations(nameof(EditLeaveTimeRequest.StartTime), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditLeaveTimeRequest.EndTime), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditLeaveTimeRequest.Minutes), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditLeaveTimeRequest.LeaveType), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditLeaveTimeRequest.IsActive), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditLeaveTimeRequest.Comment), new List<OperationType> { OperationType.Replace });

      #endregion

      #region Minutes

      AddFailureForPropertyIf(
        nameof(EditLeaveTimeRequest.Minutes),
        x => x == OperationType.Replace,
        new()
        {
          { x => int.TryParse(x.value.ToString(), out int count) && count > 0, "Incorrect format of Minutes." },
        });

      #endregion

      #region LeaveType

      AddFailureForPropertyIf(
        nameof(EditLeaveTimeRequest.LeaveType),
        x => x == OperationType.Replace,
        new()
        {
          { x => Enum.TryParse(typeof(LeaveType), x.value.ToString(), out _), "Incorrect format of LeaveType." },
        });

      #endregion

      #region IsActive

      AddFailureForPropertyIf(
        nameof(EditLeaveTimeRequest.IsActive),
        x => x == OperationType.Replace,
        new()
        {
          { x => bool.TryParse(x.value.ToString(), out _), "Incorrect format of IsActive." },
        });

      #endregion

      #region Comment

      AddFailureForPropertyIf(
        nameof(EditLeaveTimeRequest.Comment),
        x => x == OperationType.Replace,
        new()
        {
          { x => x.value is null || x.value.ToString().Length <= 500, "Comment is too long." },
        });

      #endregion
    }

    public EditLeaveTimeRequestValidator(ILeaveTimeRepository repository)
    {
      _repository = repository;

      RuleForEach(x => x.Item2.Operations)
        .Custom(HandleInternalPropertyValidation);

      //will be moved to own validator
      RuleFor(x => x)
        .CustomAsync(async (x, context, _) => await ValidateOverlappingAsync(x.Item1, x.Item2.Operations, context));
    }
  }
}
