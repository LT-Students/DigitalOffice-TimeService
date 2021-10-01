using FluentValidation;
using FluentValidation.Validators;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.Helper;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System.Collections.Generic;

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
                    nameof(EditWorkTimeRequest.UserHours),
                    nameof(EditWorkTimeRequest.ManagerHours),
                    nameof(EditWorkTimeRequest.Description)
                });

            AddСorrectOperations(nameof(EditWorkTimeRequest.UserHours), new List<OperationType> { OperationType.Replace });
            AddСorrectOperations(nameof(EditWorkTimeRequest.ManagerHours), new List<OperationType> { OperationType.Replace });
            AddСorrectOperations(nameof(EditWorkTimeRequest.Description), new List<OperationType> { OperationType.Replace });
            #endregion

            #region UserHours

            AddFailureForPropertyIf(
                nameof(EditWorkTimeRequest.UserHours),
                x => x == OperationType.Replace,
                new()
                {
                    { x => x.value == null
                        || float.TryParse(x.value.ToString(), out _), "Incorrect format of UserHours." }
                });

            #endregion

            #region ManagerHours

            AddFailureForPropertyIf(
                nameof(EditWorkTimeRequest.ManagerHours),
                x => x == OperationType.Replace,
                new()
                {
                    {
                        x => x.value == null
                          || float.TryParse(x.value.ToString(), out _), "Incorrect format of ManagerHours."
                    }
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
