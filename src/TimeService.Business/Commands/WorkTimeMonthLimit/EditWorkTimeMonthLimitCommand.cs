using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTimeMonthLimit.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Patch.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTimeMonthLimit.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTimeMonthLimit
{
  public class EditWorkTimeMonthLimitCommand : IEditWorkTimeMonthLimitCommand
    {
        private readonly IWorkTimeMonthLimitRepository _repository;
        private readonly IPatchDbWorkTimeMonthLimitMapper _mapper;
        private readonly IAccessValidator _accessValidator;
        private readonly IEditWorkTimeMonthLimitRequestValidator _validator;

        public EditWorkTimeMonthLimitCommand(
            IWorkTimeMonthLimitRepository repository,
            IPatchDbWorkTimeMonthLimitMapper mapper,
            IAccessValidator accessValidator,
            IEditWorkTimeMonthLimitRequestValidator validator)
        {
            _repository = repository;
            _mapper = mapper;
            _accessValidator = accessValidator;
            _validator = validator;
        }

        public OperationResultResponse<bool> Execute(Guid workTimeMonthLimitId, JsonPatchDocument<EditWorkTimeMonthLimitRequest> request)
        {
            if (!_accessValidator.IsAdmin() && !_accessValidator.HasRights(Rights.AddEditRemoveTime))
            {
                throw new ForbiddenException("Not enouth rights.");
            }

            _validator.ValidateAndThrowCustom(request);

            bool result = _repository.Edit(workTimeMonthLimitId, _mapper.Map(request));

            return new()
            {
                Status = result ? OperationResultStatusType.FullSuccess : OperationResultStatusType.Failed,
                Body = result,
                Errors = new()
            };
        }
    }
}
