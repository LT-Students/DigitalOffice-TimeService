using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using FluentValidation;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Validators;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.TimeService.Validation.WorkTime
{
  public class EditWorkTimeRequestValidator : ExtendedEditRequestValidator<DbWorkTime, EditWorkTimeRequest>, IEditWorkTimeRequestValidator
  {
    private bool IsDateValid(DbWorkTime dbWorkTime)
    {
      if (dbWorkTime is null)
      {
        return false;
      }

      DateTime dateTimeNow = DateTime.UtcNow;

      DateTime thisMonthFirstDay = new DateTime(dateTimeNow.Year, dateTimeNow.Month, 1);
      DateTime dbWorkTimeMonthFirstDay = new DateTime(dbWorkTime.Year, dbWorkTime.Month, 1);

      return thisMonthFirstDay == dbWorkTimeMonthFirstDay || (thisMonthFirstDay.AddMonths(-1) == dbWorkTimeMonthFirstDay && dateTimeNow.Day <= 5);
    }

    private void HandleInternalPropertyValidation(
      Operation<EditWorkTimeRequest> requestedOperation,
      ValidationContext<(DbWorkTime, JsonPatchDocument<EditWorkTimeRequest>)> context)
    {
      Context = context;
      RequestedOperation = requestedOperation;

      #region Paths

      AddСorrectPaths(
        new()
        {
          nameof(EditWorkTimeRequest.Hours),
          nameof(EditWorkTimeRequest.Description)
        });

      AddСorrectOperations(nameof(EditWorkTimeRequest.Hours), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditWorkTimeRequest.Description), new List<OperationType> { OperationType.Replace });
      #endregion

      #region UserHours

      AddFailureForPropertyIf(
        nameof(EditWorkTimeRequest.Hours),
        x => x == OperationType.Replace,
        new()
        {
          {
            x => x.value == null
              || float.TryParse(x.value.ToString(), out _),
            $"{WorkTimeValidationResource.IncorrectFormat} {nameof(EditWorkTimeRequest.Hours)}"
          }
        });

      #endregion

      #region Description

      AddFailureForPropertyIf(
        nameof(EditWorkTimeRequest.Description),
        x => x == OperationType.Replace,
        new()
        {
          { x => x.value is null || x.value.ToString().Length <= 500, $"{nameof(EditWorkTimeRequest.Description)} {WorkTimeValidationResource.LongPropertyValue}" }
        });

      #endregion
    }

    public EditWorkTimeRequestValidator(
      IHttpContextAccessor httpContextAccessor)
    {
      Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");

      RuleFor(x => x.Item1)
        .Must(wt => wt.ManagerWorkTime is null || wt.UserId != httpContextAccessor.HttpContext.GetUserId())
        .WithMessage(WorkTimeValidationResource.CannotBeEdited)
        .DependentRules(() =>
        {
          RuleForEach(x => x.Item2.Operations)
            .Custom(HandleInternalPropertyValidation);

          RuleFor(x => x.Item1)
            .Must(x => IsDateValid(x))
            .WithMessage(WorkTimeValidationResource.DateIsIncorrect);

          When(x => x.Item1.ProjectId == Guid.Empty
            && x.Item2.Operations.Any(op => op.path.EndsWith(nameof(EditWorkTimeRequest.Description), StringComparison.OrdinalIgnoreCase)),
            () =>
            {
              RuleFor(x => x.Item2.Operations.FirstOrDefault(op => op.path.EndsWith(nameof(EditWorkTimeRequest.Description), StringComparison.OrdinalIgnoreCase)))
                .Must(op => !string.IsNullOrWhiteSpace(op.value?.ToString()))
                .WithMessage($"{nameof(EditWorkTimeRequest.Description)} {WorkTimeValidationResource.EmptyValue}");
            });
        });
    }
  }
}
