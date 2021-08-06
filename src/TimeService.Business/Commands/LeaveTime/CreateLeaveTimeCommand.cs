using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Common;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime
{
    public class CreateLeaveTimeCommand : ICreateLeaveTimeCommand
    {
        private readonly ICreateLeaveTimeRequestValidator _validator;
        private readonly IDbLeaveTimeMapper _mapper;
        private readonly ILeaveTimeRepository _repository;
        private readonly IAccessValidator _accessValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateLeaveTimeCommand(
            ICreateLeaveTimeRequestValidator validator,
            IDbLeaveTimeMapper mapper,
            ILeaveTimeRepository repository,
            IAccessValidator accessValidator,
            IHttpContextAccessor httpContextAccessor,
            IRequestClient<ICheckUserExistence> rcCheckUsersExistence,
            ILogger<CreateLeaveTimeCommand> logger)
        {
            _validator = validator;
            _mapper = mapper;
            _repository = repository;
            _accessValidator = accessValidator;
        }

        public OperationResultResponse<Guid> Execute(CreateLeaveTimeRequest request)
        {
            List<string> errors = new();

            var isOwner = request.UserId == _httpContextAccessor.HttpContext.GetUserId();

            if (!isOwner && !_accessValidator.IsAdmin())
            {
                throw new ForbiddenException("Not enough rights.");
            }

            _validator.ValidateAndThrowCustom(request);

            var createdBy = _httpContextAccessor.HttpContext.GetUserId();
            var dbLeaveTime = _mapper.Map(request, createdBy);

            return new OperationResultResponse<Guid>
            {
                Body = _repository.Add(dbLeaveTime),
                Status = OperationResultStatusType.FullSuccess
            };
        }
    }
}
