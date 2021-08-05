using FluentValidation;
using FluentValidation.Validators;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.Helper;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    nameof(EditWorkTimeRequest.StartTime),
                    nameof(EditWorkTimeRequest.EndTime),
                    nameof(EditWorkTimeRequest.Minutes),
                    nameof(EditWorkTimeRequest.Title),
                    nameof(EditWorkTimeRequest.IsActive),
                    nameof(EditWorkTimeRequest.ProjectId),
                    nameof(EditWorkTimeRequest.Description)
                });

            AddСorrectOperations(nameof(EditWorkTimeRequest.StartTime), new List<OperationType> { OperationType.Replace });
            AddСorrectOperations(nameof(EditWorkTimeRequest.EndTime), new List<OperationType> { OperationType.Replace });
            AddСorrectOperations(nameof(EditWorkTimeRequest.Minutes), new List<OperationType> { OperationType.Replace });
            AddСorrectOperations(nameof(EditWorkTimeRequest.Title), new List<OperationType> { OperationType.Replace });
            AddСorrectOperations(nameof(EditWorkTimeRequest.IsActive), new List<OperationType> { OperationType.Replace });
            AddСorrectOperations(nameof(EditWorkTimeRequest.Description), new List<OperationType> { OperationType.Replace });
            AddСorrectOperations(nameof(EditWorkTimeRequest.ProjectId), new List<OperationType> { OperationType.Replace });
            #endregion

            #region StartTime

            AddFailureForPropertyIf(
                nameof(EditWorkTimeRequest.StartTime),
                x => x == OperationType.Replace,
                new()
                {
                    { x => DateTime.TryParse(x.value.ToString(), out _), "Incorrect format of StartTime." },
                });

            #endregion

            #region EndTime

            AddFailureForPropertyIf(
                nameof(EditWorkTimeRequest.EndTime),
                x => x == OperationType.Replace,
                new()
                {
                    { x => DateTime.TryParse(x.value.ToString(), out _), "Incorrect format of EndTime." },
                });

            #endregion

            #region Minutes

            AddFailureForPropertyIf(
                nameof(EditWorkTimeRequest.Minutes),
                x => x == OperationType.Replace,
                new()
                {
                    { x => int.TryParse(x.value.ToString(), out _), "Incorrect format of Minutes." },
                });

            #endregion

            #region IsActive

            AddFailureForPropertyIf(
                nameof(EditWorkTimeRequest.IsActive),
                x => x == OperationType.Replace,
                new()
                {
                    { x => bool.TryParse(x.value.ToString(), out _), "Incorrect format of IsActive." },
                });

            #endregion

            #region ProjectId

            AddFailureForPropertyIf(
                nameof(EditWorkTimeRequest.IsActive),
                x => x == OperationType.Replace,
                new()
                {
                    { x => Guid.TryParse(x.value.ToString(), out _), "Incorrect format of ProjectId." },
                });

            #endregion
        }

        public EditWorkTimeRequestValidator()
        {
            RuleForEach(x => x.Operations)
               .Custom(HandleInternalPropertyValidation);

            When(x => x.Operations.Any(o => o.path.EndsWith(nameof(EditWorkTimeRequest.StartTime), StringComparison.OrdinalIgnoreCase))
                && x.Operations.Any(o => o.path.EndsWith(nameof(EditWorkTimeRequest.EndTime), StringComparison.OrdinalIgnoreCase)),
                () =>
                {
                    RuleFor(x => x)
                        .Must(x =>
                        {
                            DateTime start = DateTime.Parse(x.Operations
                                .FirstOrDefault(o => o.path.EndsWith(nameof(EditWorkTimeRequest.StartTime), StringComparison.OrdinalIgnoreCase))
                                .value
                                .ToString());

                            DateTime end = DateTime.Parse(x.Operations
                                .FirstOrDefault(o => o.path.EndsWith(nameof(EditWorkTimeRequest.EndTime), StringComparison.OrdinalIgnoreCase))
                                .value
                                .ToString());

                            return start < end;
                        })
                        .WithMessage("Start time should be less then end.");
                });
        }
    }
}
