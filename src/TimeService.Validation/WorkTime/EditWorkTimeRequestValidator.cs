using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Validators;
using LT.DigitalOffice.Kernel.Validators;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.TimeService.Validation.WorkTime
{
  public class EditWorkTimeRequestValidator : BaseEditRequestValidator<EditWorkTimeRequest>, IEditWorkTimeRequestValidator
  {
    private void HandleInternalPropertyValidation(Operation<EditWorkTimeRequest> requestedOperation, CustomContext context)
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
            "Incorrect format of Hours."
          }
        });

      #endregion

      #region Description

      AddFailureForPropertyIf(
        nameof(EditWorkTimeRequest.Description),
        x => x == OperationType.Replace,
        new()
        {
          { x => x.value is null || x.value?.ToString().Length <= 500, "Description is too long."}
        });

      #endregion
    }

    public EditWorkTimeRequestValidator()
    {
      RuleForEach(x => x.Operations)
        .Custom(HandleInternalPropertyValidation);
    }
  }
}
