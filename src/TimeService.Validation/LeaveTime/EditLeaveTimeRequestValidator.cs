﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using FluentValidation;
using FluentValidation.Validators;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Validators;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.TimeService.Validation.LeaveTime
{
  public class EditLeaveTimeRequestValidator : ExtendedEditRequestValidator<DbLeaveTime, EditLeaveTimeRequest>, IEditLeaveTimeRequestValidator
  {
    private void HandleInternalPropertyValidation(
      Operation<EditLeaveTimeRequest> requestedOperation,
      ValidationContext<(DbLeaveTime, JsonPatchDocument<EditLeaveTimeRequest>)> context)
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
          nameof(EditLeaveTimeRequest.IsClosed),
          nameof(EditLeaveTimeRequest.IsActive),
          nameof(EditLeaveTimeRequest.Comment)
        });

      AddСorrectOperations(nameof(EditLeaveTimeRequest.StartTime), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditLeaveTimeRequest.EndTime), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditLeaveTimeRequest.Minutes), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditLeaveTimeRequest.IsClosed), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditLeaveTimeRequest.IsActive), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditLeaveTimeRequest.Comment), new List<OperationType> { OperationType.Replace });

      #endregion

      #region Minutes

      AddFailureForPropertyIf(
        nameof(EditLeaveTimeRequest.Minutes),
        x => x == OperationType.Replace,
        new()
        {
          { x => int.TryParse(x.value.ToString(), out int count) && count > 0, $"{LeaveTimeValidatorResource.IncorrectFormat} {nameof(EditLeaveTimeRequest.Minutes)}" },
        });

      #endregion

      #region IsClosed

      AddFailureForPropertyIf(
        nameof(EditLeaveTimeRequest.IsClosed),
        x => x == OperationType.Replace,
        new()
        {
          {
            x => bool.TryParse(x.value.ToString(), out _),
            $"{LeaveTimeValidatorResource.IncorrectFormat} {nameof(EditLeaveTimeRequest.IsClosed)}"
          },
        });

      #endregion

      #region IsActive

      AddFailureForPropertyIf(
        nameof(EditLeaveTimeRequest.IsActive),
        x => x == OperationType.Replace,
        new()
        {
          { x => bool.TryParse(x.value.ToString(), out _), $"{LeaveTimeValidatorResource.IncorrectFormat} {nameof(EditLeaveTimeRequest.IsActive)}" },
        });

      #endregion

      #region Comment

      AddFailureForPropertyIf(
        nameof(EditLeaveTimeRequest.Comment),
        x => x == OperationType.Replace,
        new()
        {
          { x => x.value is null || x.value.ToString().Length <= 500, $"{nameof(EditLeaveTimeRequest.Comment)} {LeaveTimeValidatorResource.LongPropertyValue}" },
        });

      #endregion
    }

    private void ValidateProlongedFailures(
      DbLeaveTime dbLeaveTime,
      List<Operation<EditLeaveTimeRequest>> operations,
      ValidationContext<(DbLeaveTime, JsonPatchDocument<EditLeaveTimeRequest>)> context)
    {
      Operation<EditLeaveTimeRequest> isCLosedOperation = operations.FirstOrDefault(
        o => o.path.EndsWith(nameof(EditLeaveTimeRequest.IsClosed), StringComparison.OrdinalIgnoreCase));
      Operation<EditLeaveTimeRequest> endTimeOperation = operations.FirstOrDefault(
        o => o.path.EndsWith(nameof(EditLeaveTimeRequest.EndTime), StringComparison.OrdinalIgnoreCase));

      bool isValid;

      // isClosed field can be edited only in prolonged leave times, in others it is always true
      isValid = isCLosedOperation is not null
        ? dbLeaveTime.LeaveType == (int)LeaveType.Prolonged
        : true;

      if (!isValid)
      {
        context.AddFailure(LeaveTimeValidatorResource.IsClosedFailure);
      }

      // end time can't be edited in prolonged leave time without closing it
      if (isValid && endTimeOperation is not null)
      {
        isValid = dbLeaveTime.IsClosed
          || isCLosedOperation is not null && bool.TryParse(isCLosedOperation.value.ToString(), out bool isClosed) && isClosed;

        if (!isValid)
        {
          context.AddFailure(LeaveTimeValidatorResource.EndTimeInProlonged);
        }
      }
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
    private (DateTimeOffset? startTime, DateTimeOffset? endTime, DbLeaveTime leaveTime, Guid? userId) GetItems(
      DbLeaveTime oldLeaveTime,
      List<Operation<EditLeaveTimeRequest>> operations)
    {
      Operation<EditLeaveTimeRequest> startTimeOperation = operations.FirstOrDefault(
        o => o.path.EndsWith(nameof(EditLeaveTimeRequest.StartTime), StringComparison.OrdinalIgnoreCase));
      Operation<EditLeaveTimeRequest> endTimeOperation = operations.FirstOrDefault(
        o => o.path.EndsWith(nameof(EditLeaveTimeRequest.EndTime), StringComparison.OrdinalIgnoreCase));

      DateTimeOffset? startTime = startTimeOperation is not null
        ? DateTimeOffset.Parse(startTimeOperation.value.ToString())
        : null;
      DateTimeOffset? endTime = endTimeOperation is not null
        ? DateTimeOffset.Parse(endTimeOperation?.value.ToString())
        : null;

      return (startTime: startTime, endTime: endTime, leaveTime: oldLeaveTime, userId: null);
    }

    public EditLeaveTimeRequestValidator(
      ILeaveTimeIntervalValidator validator,
      IHttpContextAccessor httpContextAccessor)
    {
      Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");

      RuleFor(x => x.Item1)
        .Must(x => x.ManagerLeaveTime == null || x.UserId != httpContextAccessor.HttpContext.GetUserId())
        .WithMessage(LeaveTimeValidatorResource.CannotBeEdited)
        .DependentRules(() =>
        {
          RuleForEach(x => x.Item2.Operations)
            .Custom(HandleInternalPropertyValidation);

          RuleFor(x => x.Item2.Operations)
            .Must(ops => ValidateTimeVariables(ops))
            .WithMessage($"{LeaveTimeValidatorResource.IncorrectFormat} {nameof(EditLeaveTimeRequest.StartTime)} or {nameof(EditLeaveTimeRequest.EndTime)}")
            .DependentRules(() =>
            {
              RuleFor(x => GetItems(x.Item1, x.Item2.Operations))
                .SetValidator(validator);
            });

          RuleFor(x => x)
            .Custom((x, context) => ValidateProlongedFailures(x.Item1, x.Item2.Operations, context));
        });
    }
  }
}
