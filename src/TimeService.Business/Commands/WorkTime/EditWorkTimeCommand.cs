using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Patch.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime
{
  public class EditWorkTimeCommand: IEditWorkTimeCommand
    {
        private readonly IEditWorkTimeRequestValidator _validator;
        private readonly IWorkTimeRepository _repository;
        private readonly IPatchDbWorkTimeMapper _mapper;
        private readonly IAccessValidator _accessValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EditWorkTimeCommand(
            IEditWorkTimeRequestValidator validator,
            IWorkTimeRepository repository,
            IPatchDbWorkTimeMapper mapper,
            IAccessValidator accessValidator,
            IHttpContextAccessor httpContextAccessor)
        {
            _validator = validator;
            _repository = repository;
            _mapper = mapper;
            _accessValidator = accessValidator;
            _httpContextAccessor = httpContextAccessor;
        }

        public OperationResultResponse<bool> Execute(Guid workTimeId, JsonPatchDocument<EditWorkTimeRequest> request)
        {
            var oldDbWorkTime = _repository.Get(workTimeId);

            var isOwner = _httpContextAccessor.HttpContext.GetUserId() == oldDbWorkTime.UserId;
            if (!isOwner && !_accessValidator.IsAdmin() && !_accessValidator.HasRights(Rights.AddEditRemoveTime))
            {
                throw new ForbiddenException("Not enough rights.");
            }

            _validator.ValidateAndThrowCustom(request);

            return new OperationResultResponse<bool>
            {
                Body = _repository.Edit(oldDbWorkTime, _mapper.Map(request)),
                Status = OperationResultStatusType.FullSuccess,
                Errors = new()
            };
        }
    }
}
