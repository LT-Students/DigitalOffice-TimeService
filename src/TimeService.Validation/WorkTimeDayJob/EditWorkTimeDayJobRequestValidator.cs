using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using LT.DigitalOffice.Kernel.Validators;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTimeDayJob.Interfaces;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.TimeService.Validation.WorkTimeDayJob
{
  public class EditWorkTimeDayJobRequestValidator : BaseEditRequestValidator<EditWorkTimeDayJobRequest>, IEditWorkTimeDayJobRequestValidator
  {
    private readonly IWorkTimeRepository _repository;

    private async Task HandleInternalPropertyValidationAsync(Operation<EditWorkTimeDayJobRequest> requestedOperation, CustomContext context)
    {
      Context = context;
      RequestedOperation = requestedOperation;

      #region Paths

      AddСorrectPaths(
        new()
        {
          nameof(EditWorkTimeDayJobRequest.Name),
          nameof(EditWorkTimeDayJobRequest.Day),
          nameof(EditWorkTimeDayJobRequest.Description),
          nameof(EditWorkTimeDayJobRequest.Minutes),
          nameof(EditWorkTimeDayJobRequest.IsActive),
          nameof(EditWorkTimeDayJobRequest.WorkTimeId)
        });

      AddСorrectOperations(nameof(EditWorkTimeDayJobRequest.Name), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditWorkTimeDayJobRequest.Day), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditWorkTimeDayJobRequest.Description), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditWorkTimeDayJobRequest.Minutes), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditWorkTimeDayJobRequest.IsActive), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditWorkTimeDayJobRequest.WorkTimeId), new List<OperationType> { OperationType.Replace });
      #endregion

      #region Name

      AddFailureForPropertyIf(
        nameof(EditWorkTimeDayJobRequest.Name),
        x => x == OperationType.Replace,
        new()
        {
          {
            x => !string.IsNullOrEmpty(x.value.ToString()?.Trim()),
            "Name cannot be empty."
          }
        });

      #endregion

      #region Day

      AddFailureForPropertyIf(
        nameof(EditWorkTimeDayJobRequest.Day),
        x => x == OperationType.Replace,
        new()
        {
          {
            x => int.TryParse(x.value.ToString(), out int day) && day > 0 && day < 32,
            "Incorrect format of day."
          }
        });

      #endregion

      #region Minutes

      AddFailureForPropertyIf(
        nameof(EditWorkTimeDayJobRequest.Minutes),
        x => x == OperationType.Replace,
        new()
        {
          {
            x => int.TryParse(x.value.ToString(), out int minutes) && minutes > 0,
            "Incorrect format of minutes."
          }
        });

      #endregion

      #region IsActive

      AddFailureForPropertyIf(
        nameof(EditWorkTimeDayJobRequest.IsActive),
        x => x == OperationType.Replace,
        new()
        {
          {
            x => bool.TryParse(x.value.ToString(), out _),
            "Incorrect format of IsActive."
          }
        });

      #endregion

      #region WorkTimeId

      await AddFailureForPropertyIfAsync(
        nameof(EditWorkTimeDayJobRequest.WorkTimeId),
        x => x == OperationType.Replace,
        new()
        {
          {
            async x => Guid.TryParse(x.value.ToString(), out Guid id) && await _repository.DoesExistAsync(id),
            "Incorrect worktime id."
          }
        });

      #endregion
    }

    public EditWorkTimeDayJobRequestValidator(IWorkTimeRepository repository)
    {
      _repository = repository;

      RuleForEach(x => x.Operations)
        .CustomAsync(async (op, context, _) => await HandleInternalPropertyValidationAsync(op, context));
    }
  }
}
