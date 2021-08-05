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
using LT.DigitalOffice.TimeService.Validation.Interfaces;
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
        private readonly IRequestClient<ICheckUserExistence> _rcCheckUsersExistence;
        private readonly ILogger<CreateLeaveTimeCommand> _logger;

        private ICheckUserExistence CheckUserExistence(List<Guid> userIds, List<string> errors)
        {
            string errorMessage = "Failed to check the existing users. Please try again later ";
            string logMessage = "Cannot check existing users {userIds}";

            try
            {
                var response = _rcCheckUsersExistence.GetResponse<IOperationResult<ICheckUserExistence>>(
                    ICheckUserExistence.CreateObj(userIds)).Result;
                if (response.Message.IsSuccess)
                {
                    return response.Message.Body;
                }

                _logger.LogWarning($"Can not find users with these Ids '{userIds}': " +
                    $"{Environment.NewLine}{string.Join('\n', response.Message.Errors)}");
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, logMessage);

                errors.Add(errorMessage);
            }
            return null;
        }

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
            _httpContextAccessor = httpContextAccessor;
            _rcCheckUsersExistence = rcCheckUsersExistence;
            _logger = logger;
        }

        public OperationResultResponse<Guid> Execute(CreateLeaveTimeRequest request)
        {
            List<string> errors = new();

            List<Guid> userIds = new(){request.UserId};

            OperationResultResponse<Guid> response = new();

            var isAuthor = request.UserId == _httpContextAccessor.HttpContext.GetUserId();

            if (!isAuthor && !_accessValidator.IsAdmin())
            {
                throw new ForbiddenException("Not enough rights.");
            }

            _validator.ValidateAndThrowCustom(request);

            var existUsers = CheckUserExistence(userIds, errors);
            if (existUsers.UserIds.Count == 0 )
            {
                response.Status = OperationResultStatusType.Failed;
                response.Errors.Add("Project users don't exist.");
                return response;
            }

            var createdBy = _httpContextAccessor.HttpContext.GetUserId();
            var dbLeaveTime = _mapper.Map(request, createdBy, existUsers.UserIds);

            return new OperationResultResponse<Guid>
            {
                Body = _repository.Add(dbLeaveTime),
                Status = OperationResultStatusType.FullSuccess
            };
        }
    }
}
