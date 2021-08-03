using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime
{
    public class EditWorkTimeCommand: IEditWorkTimeCommand
    {
        private readonly IEditWorkTimeRequestValidator _validator;
        private readonly IWorkTimeRepository _repository;
        private readonly IPatchDbWorkTimeMapper _mapper;
        private readonly IAccessValidator _accessValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRequestClient<IGetUserProjectsRequest> _rcGetUserProjects;
        private readonly ILogger<ICreateWorkTimeRequestValidator> _logger;

        private bool IsUserInProject(Guid userId, Guid projectId)
        {
            const string logMessage = "Cannot check including user '{UserId}' to project '{ProjectId}'.";

            try
            {
                IOperationResult<IProjectsResponse> response = _rcGetUserProjects
                    .GetResponse<IOperationResult<IProjectsResponse>>(IGetUserProjectsRequest.CreateObj(userId)).Result.Message;

                if (response.IsSuccess && response.Body.ProjectsIds.Contains(projectId))
                {
                    return true;
                }

                _logger.LogWarning(
                    logMessage + "Reason:\n{Errors}",
                    userId,
                    projectId,
                    string.Join(',', response.Errors));
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, logMessage, userId, projectId);
            }

            return false;
        }

        public EditWorkTimeCommand(
            IEditWorkTimeRequestValidator validator,
            IWorkTimeRepository repository,
            IPatchDbWorkTimeMapper mapper,
            IAccessValidator accessValidator,
            IHttpContextAccessor httpContextAccessor,
            IRequestClient<IGetUserProjectsRequest> rcGetUserProjects,
            ILogger<ICreateWorkTimeRequestValidator> logger)
        {
            _validator = validator;
            _repository = repository;
            _mapper = mapper;
            _accessValidator = accessValidator;
            _httpContextAccessor = httpContextAccessor;
            _rcGetUserProjects = rcGetUserProjects;
            _logger = logger;
        }

        public OperationResultResponse<bool> Execute(Guid workTimeId, JsonPatchDocument<EditWorkTimeRequest> request)
        {
            var oldDbWorkTime = _repository.Get(workTimeId);

            var isOwner = _httpContextAccessor.HttpContext.GetUserId() == oldDbWorkTime.UserId;
            if (!isOwner && !_accessValidator.IsAdmin())
            {
                throw new ForbiddenException("Not enough rights.");
            }

            _validator.ValidateAndThrowCustom(request);

            Operation<EditWorkTimeRequest> projectOperation = request.Operations.FirstOrDefault(
                o => o.path.EndsWith(nameof(EditWorkTimeRequest.ProjectId), StringComparison.OrdinalIgnoreCase));
            if (projectOperation != null && !IsUserInProject(oldDbWorkTime.UserId, Guid.Parse(projectOperation.value.ToString())))
            {
                throw new BadRequestException("The user must participate in the project.");
            }

            return new OperationResultResponse<bool>
            {
                Body = _repository.Edit(oldDbWorkTime, _mapper.Map(request)),
                Status = OperationResultStatusType.FullSuccess,
                Errors = new()
            };
        }
    }
}
