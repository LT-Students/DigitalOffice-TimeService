using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime
{
    public class EditLeaveTimeCommand : IEditLeaveTimeCommand
    {
        private readonly IEditLeaveTimeRequestValidator _validator;
        private readonly ILeaveTimeRepository _repository;
        private readonly IPatchDbLeaveTimeMapper _mapper;
        private readonly IAccessValidator _accessValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EditLeaveTimeCommand(
            IEditLeaveTimeRequestValidator validator,
            ILeaveTimeRepository repository,
            IPatchDbLeaveTimeMapper mapper,
            IAccessValidator accessValidator,
            IHttpContextAccessor httpContextAccessor)
        {
            _validator = validator;
            _repository = repository;
            _mapper = mapper;
            _accessValidator = accessValidator;
            _httpContextAccessor = httpContextAccessor;
        }

        public OperationResultResponse<bool> Execute(Guid leaveTimeId, JsonPatchDocument<EditLeaveTimeRequest> request)
        {
            var oldDbWorkTime = _repository.Get(leaveTimeId);

            var isAuthor = _httpContextAccessor.HttpContext.GetUserId() == oldDbWorkTime.CreatedBy;
            if (!isAuthor && !_accessValidator.IsAdmin())
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
