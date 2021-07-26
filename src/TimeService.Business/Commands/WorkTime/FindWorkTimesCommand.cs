using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime
{
    public class FindWorkTimesCommand : IFindWorkTimesCommand
    {
        private readonly IWorkTimeInfoMapper _mapper;
        private readonly IWorkTimeRepository _repository;
        private readonly IAccessValidator _accessValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRequestClient<IFindProjectsRequest> _rcFindProjects;
        private readonly ILogger<FindWorkTimesCommand> _logger;
        private readonly IProjectInfoMapper _projectInfoMapper;

        private List<ProjectData> FindProjects(List<Guid> projectIds, List<string> errors)
        {
            string messageError = "Cannot get projects info. Please, try again later.";
            const string logError = "Cannot get projects info with ids: {projectIds}.";

            try
            {
                IOperationResult<IFindProjectsResponse> result = _rcFindProjects.GetResponse<IOperationResult<IFindProjectsResponse>>(
                    IFindProjectsRequest.CreateObj(projectIds)).Result.Message;

                if (result.IsSuccess)
                {
                    return result.Body.Projects;
                }

                _logger.LogWarning(logError + "Errors: {errors}.", string.Join(", ", projectIds), string.Join("\n", result.Errors));
            }
            catch(Exception exc)
            {
                _logger.LogError(exc, logError, string.Join(", ", projectIds));
            }

            errors.Add(messageError);
            return null;
        }

        public FindWorkTimesCommand(
            IWorkTimeInfoMapper mapper,
            IWorkTimeRepository repository,
            IAccessValidator accessValidator,
            IHttpContextAccessor httpContextAccessor,
            IRequestClient<IFindProjectsRequest> rcFindProjects,
            ILogger<FindWorkTimesCommand> logger,
            IProjectInfoMapper projectInfoMapper)
        {
            _mapper = mapper;
            _repository = repository;
            _accessValidator = accessValidator;
            _httpContextAccessor = httpContextAccessor;
            _rcFindProjects = rcFindProjects;
            _logger = logger;
            _projectInfoMapper = projectInfoMapper;
        }

        public FindResultResponse<WorkTimeInfo> Execute(FindWorkTimesFilter filter, int skipCount, int takeCount)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var isActhor = filter.UserId.HasValue && filter.UserId == _httpContextAccessor.HttpContext.GetUserId();

            if (!isActhor && !_accessValidator.IsAdmin())
            {
                throw new ForbiddenException("Not enough rights.");
            }

            List<string> errors = new();

            var dbWorkTimes = _repository.Find(filter, skipCount, takeCount, out int totalCount);

            List<Guid> projectIds = new();
            foreach(Guid id in dbWorkTimes.Select(wt => wt.ProjectId).ToList())
            {
                if (!projectIds.Contains(id))
                {
                    projectIds.Add(id);
                }
            };

            List<ProjectData> projects = FindProjects(projectIds, errors);

            return new()
            {
                Status = errors.Any() ? OperationResultStatusType.PartialSuccess : OperationResultStatusType.FullSuccess,
                TotalCount = totalCount,
                Body = dbWorkTimes.Select(
                    wt => _mapper.Map(wt, _projectInfoMapper.Map(projects?.FirstOrDefault(p => p.Id == wt.ProjectId)))).ToList(),
                Errors = errors
            };
        }
    }
}
