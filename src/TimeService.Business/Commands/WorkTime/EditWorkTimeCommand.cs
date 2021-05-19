using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Models.Dto.Requests.HelpersModels;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
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

        public bool Execute(Guid workTimeId, JsonPatchDocument<EditWorkTimeRequest> request)
        {
            var editModel = new EditWorkTimeModel
            {
                JsonPatchDocument = request,
                Id = workTimeId,
                UserId = _httpContextAccessor.HttpContext.GetUserId()
            };

            _validator.ValidateAndThrowCustom(editModel);

            var oldDbWorkTime = _repository.GetWorkTime(workTimeId);
            var isAuthor = _httpContextAccessor.HttpContext.GetUserId() == oldDbWorkTime.CreatedBy;
            if (!_accessValidator.IsAdmin() && !isAuthor)
            {
                throw new ForbiddenException("Not enough rights.");
            }

            var response = new OperationResultResponse<bool>();

            return _repository.Edit(oldDbWorkTime, _mapper.Map(request));
        }
    }
}
