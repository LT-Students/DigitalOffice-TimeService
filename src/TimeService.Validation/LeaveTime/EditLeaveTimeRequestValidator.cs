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

    private bool ValidateTimeVariables(List<Operation<EditLeaveTimeRequest>> operations)
    {
      Operation<EditLeaveTimeRequest> startTimeOperation = operations.FirstOrDefault(
        o => o.path.EndsWith(nameof(EditLeaveTimeRequest.StartTime), StringComparison.OrdinalIgnoreCase));
      Operation<EditLeaveTimeRequest> endTimeOperation = operations.FirstOrDefault(
        o => o.path.EndsWith(nameof(EditLeaveTimeRequest.EndTime), StringComparison.OrdinalIgnoreCase));

      return (startTimeOperation is null || DateTimeOffset.TryParse(startTimeOperation.value.ToString(), out _))
        && (endTimeOperation is null || DateTimeOffset.TryParse(endTimeOperation.value.ToString(), out _));
    }

    //user id is always null here, it is used for time validation in createLeaveTimeRequest
    private (DateTimeOffset startTime, DateTimeOffset endTime, DbLeaveTime leaveTime, Guid? userId) GetItems(
      DbLeaveTime oldLeaveTime,
      List<Operation<EditLeaveTimeRequest>> operations)
    {
      Operation<EditLeaveTimeRequest> startTimeOperation = operations.FirstOrDefault(
        o => o.path.EndsWith(nameof(EditLeaveTimeRequest.StartTime), StringComparison.OrdinalIgnoreCase));
      Operation<EditLeaveTimeRequest> endTimeOperation = operations.FirstOrDefault(
        o => o.path.EndsWith(nameof(EditLeaveTimeRequest.EndTime), StringComparison.OrdinalIgnoreCase));

      DateTimeOffset startTime;
      DateTimeOffset endTime;

      if (startTimeOperation is null)
      {
        endTime = DateTimeOffset.Parse(endTimeOperation.value.ToString());
        startTime = new DateTimeOffset(DateTime.SpecifyKind(oldLeaveTime.StartTime, DateTimeKind.Unspecified), endTime.Offset);
      }
      else
      {
        startTime = DateTimeOffset.Parse(startTimeOperation.value.ToString());
        endTime = endTimeOperation is null
          ? new DateTimeOffset(DateTime.SpecifyKind(oldLeaveTime.EndTime, DateTimeKind.Unspecified), startTime.Offset)
          : DateTimeOffset.Parse(endTimeOperation.value.ToString());
      }

      return (startTime: startTime, endTime: endTime, leaveTime: oldLeaveTime, userId: null);
    }

    public EditLeaveTimeRequestValidator(ILeaveTimeIntervalValidator validator)
    {
      RuleForEach(x => x.Item2.Operations)
        .Custom(HandleInternalPropertyValidation);

      RuleFor(x => x.Item2.Operations)
        .Must(ops => ValidateTimeVariables(ops))
        .WithMessage("Incorrect format of startTime or endTime.")
        .DependentRules(() =>
        {
          RuleFor(x => GetItems(x.Item1, x.Item2.Operations))
            .SetValidator(validator);
        });
    }
  }
}
