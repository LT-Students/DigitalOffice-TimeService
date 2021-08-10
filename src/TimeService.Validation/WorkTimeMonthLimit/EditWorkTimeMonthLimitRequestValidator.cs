using FluentValidation;
using FluentValidation.Validators;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.Helper;
using LT.DigitalOffice.TimeService.Validation.WorkTimeMonthLimit.Interfaces;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LT.DigitalOffice.TimeService.Validation.WorkTimeMonthLimit
{
    public class EditWorkTimeMonthLimitRequestValidator
        : BaseEditRequestValidator<EditWorkTimeMonthLimitRequest>, IEditWorkTimeMonthLimitRequestValidator
    {
        private void HandleInternalPropertyValidation(Operation<EditWorkTimeMonthLimitRequest> requestedOperation, CustomContext context)
        {
            Context = context;
            RequestedOperation = requestedOperation;

            #region Paths

            AddСorrectPaths(
                new()
                {
                    nameof(EditWorkTimeMonthLimitRequest.NormHours),
                    nameof(EditWorkTimeMonthLimitRequest.Holidays)
                });

            AddСorrectOperations(nameof(EditWorkTimeMonthLimitRequest.NormHours), new List<OperationType> { OperationType.Replace });
            AddСorrectOperations(nameof(EditWorkTimeMonthLimitRequest.Holidays), new List<OperationType> { OperationType.Replace });
            #endregion

            #region NormHours

            AddFailureForPropertyIf(
                nameof(EditWorkTimeMonthLimitRequest.NormHours),
                x => x == OperationType.Replace,
                new()
                {
                    {
                        x => x.value == null
                          || float.TryParse(x.value.ToString(), out _),
                        "Incorrect format of NormHours."
                    }
                });

            #endregion

            #region Holidays

            AddFailureForPropertyIf(
                nameof(EditWorkTimeMonthLimitRequest.Holidays),
                x => x == OperationType.Replace,
                new()
                {
                    {
                        x => !string.IsNullOrEmpty(x.value?.ToString()?.Trim()),
                        "Holidays cannot be empty."
                    },
                    {
                        x =>
                        {
                            string value = x.value?.ToString()?.Trim();

                            return value?.Length > 27 && value?.Length < 32;
                        },
                        "Incorrect size of month."
                    },
                    {
                        x =>
                        {
                            Regex regex = new("^[10]+");

                            return regex.IsMatch(x.value?.ToString()?.Trim());
                        },
                        "Incorrect format of holidays."
                    }
                });

            #endregion
        }

        public EditWorkTimeMonthLimitRequestValidator()
        {
            RuleForEach(x => x.Operations)
               .Custom(HandleInternalPropertyValidation);
        }
    }
}
