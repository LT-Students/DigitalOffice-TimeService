using FluentValidation;
using FluentValidation.Validators;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.Helper;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Validation.LeaveTime
{
    public class EditLeaveTimeRequestValidator : BaseEditRequestValidator<EditLeaveTimeRequest>, IEditLeaveTimeRequestValidator
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

            #region StartTime

            AddFailureForPropertyIf(
                nameof(EditLeaveTimeRequest.StartTime),
                x => x == OperationType.Replace,
                new()
                {
                    { x => DateTime.TryParse(x.value.ToString(), out _), "Incorrect format of StartTime." },
                });

            #endregion

            #region EndTime

            AddFailureForPropertyIf(
                nameof(EditLeaveTimeRequest.EndTime),
                x => x == OperationType.Replace,
                new()
                {
                    { x => DateTime.TryParse(x.value.ToString(), out _), "Incorrect format of EndTime." },
                });

            #endregion

            #region Minutes

            AddFailureForPropertyIf(
                nameof(EditLeaveTimeRequest.Minutes),
                x => x == OperationType.Replace,
                new()
                {
                    { x => int.TryParse(x.value.ToString(), out _), "Incorrect format of Minutes." },
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
        }

        public EditLeaveTimeRequestValidator()
        {
            RuleForEach(x => x.Operations)
               .Custom(HandleInternalPropertyValidation);

            When(x => x.Operations.Any(o => o.path.EndsWith(nameof(EditLeaveTimeRequest.StartTime), StringComparison.OrdinalIgnoreCase))
                && x.Operations.Any(o => o.path.EndsWith(nameof(EditLeaveTimeRequest.EndTime), StringComparison.OrdinalIgnoreCase)),
                () =>
                    {
                        RuleFor(x => x)
                            .Must(x =>
                            {
                                DateTime start = DateTime.Parse(x.Operations
                                    .FirstOrDefault(o => o.path.EndsWith(nameof(EditLeaveTimeRequest.StartTime), StringComparison.OrdinalIgnoreCase))
                                    .value
                                    .ToString());

                                DateTime end = DateTime.Parse(x.Operations
                                    .FirstOrDefault(o => o.path.EndsWith(nameof(EditLeaveTimeRequest.EndTime), StringComparison.OrdinalIgnoreCase))
                                    .value
                                    .ToString());

                                return start < end;
                            })
                            .WithMessage("Start time should be less then end.");
                    });
        }
    }
}
