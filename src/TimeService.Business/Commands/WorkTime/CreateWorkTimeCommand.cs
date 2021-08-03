using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces;
using Microsoft.AspNetCore.Http;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime
{
    public class CreateWorkTimeCommand : ICreateWorkTimeCommand
    {
        private readonly ICreateWorkTimeRequestValidator _validator;
        private readonly IDbWorkTimeMapper _mapper;
        private readonly IWorkTimeRepository _repository;
        private readonly IAccessValidator _accessValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateWorkTimeCommand(
            ICreateWorkTimeRequestValidator validator,
            IDbWorkTimeMapper mapper,
            IWorkTimeRepository repository,
            IAccessValidator accessValidator,
            IHttpContextAccessor httpContextAccessor)
        {
            _validator = validator;
            _mapper = mapper;
            _repository = repository;
            _accessValidator = accessValidator;
            _httpContextAccessor = httpContextAccessor;
        }

        public OperationResultResponse<Guid> Execute(CreateWorkTimeRequest request)
        {
            var isOwner = request.UserId == _httpContextAccessor.HttpContext.GetUserId();

            if (!isOwner && !_accessValidator.IsAdmin())
            {
                throw new ForbiddenException("Not enough rights.");
            }

            _validator.ValidateAndThrowCustom(request);

            var createdBy = _httpContextAccessor.HttpContext.GetUserId();
            var dbWorkTime = _mapper.Map(request, createdBy);

            return new OperationResultResponse<Guid>
            {
                Body = _repository.Create(dbWorkTime),
                Status = OperationResultStatusType.FullSuccess
            };
        }
    }
}
