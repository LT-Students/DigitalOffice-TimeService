using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using LT.DigitalOffice.Models.Broker.Responses.User;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime
{
    public class FindWorkTimesCommand : IFindWorkTimesCommand
    {
        private const string ProjectUserCacheKey = "ProjectUserCache";
        private static TimeSpan _cacheLifePeriod = new(days: 31, hours: 0, minutes: 0, seconds: 0);

        private readonly IWorkTimeInfoMapper _mapper;
        private readonly IWorkTimeRepository _workTimeRepository;
        private readonly IWorkTimeMonthLimitRepository _monthLimitRepository;
        private readonly IAccessValidator _accessValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRequestClient<IFindProjectsRequest> _rcFindProjects;
        private readonly IRequestClient<IGetUsersDataRequest> _rcGetUsers;
        private readonly IRequestClient<IGetProjectUsersRequest> _rcGetProjectUsers;
        private readonly ILogger<FindWorkTimesCommand> _logger;
        private readonly IProjectInfoMapper _projectInfoMapper;
        private readonly IUserInfoMapper _userInfoMapper;
        private readonly IWorkTimeMonthLimitInfoMapper _monthLimitMapper;
        private readonly IMemoryCache _memoryCache;

        private List<ProjectInfo> FindProjects(List<Guid> projectIds, List<string> errors)
        {
            string messageError = "Cannot get projects info. Please, try again later.";
            const string logError = "Cannot get projects info with ids: {projectIds}.";

            if (projectIds == null || projectIds.Count == 0)
            {
                return null;
            }

            try
            {
                IOperationResult<IFindProjectsResponse> result = _rcFindProjects.GetResponse<IOperationResult<IFindProjectsResponse>>(
                    IFindProjectsRequest.CreateObj(projectIds)).Result.Message;

                if (result.IsSuccess)
                {
                    return result.Body.Projects.Select(_projectInfoMapper.Map).ToList();
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

        private List<ProjectUserData> GetProjectUsers(List<(Guid projectId, Guid userId)> projectUsers, List<string> errors)
        {
            string messageError = "Cannot get project users info. Please, try again later.";
            const string logError = "Cannot get project user info for (projectId, userId): {project users}.";

            if (projectUsers == null || projectUsers.Count == 0)
            {
                return new();
            }

            List<(Guid projectId, Guid userId)> request = new();
            List<ProjectUserData> response = new();
            List<ProjectUserData> cache;

            cache = _memoryCache.Get<List<ProjectUserData>>(ProjectUserCacheKey) ?? new();

            foreach (var projectUser in projectUsers)
            {
                ProjectUserData projectUserData = cache
                    .FirstOrDefault(pud => pud.ProjectId == projectUser.projectId && pud.UserId == projectUser.userId);

                if (projectUserData == null)
                {
                    request.Add(projectUser);
                }
                else
                {
                    response.Add(projectUserData);
                }
            }

            if (request.Count == 0)
            {
                return response;
            }

            try
            {
                IOperationResult<IGetProjectUsersResponse> result = _rcGetProjectUsers.GetResponse<IOperationResult<IGetProjectUsersResponse>>(
                    IGetProjectUsersRequest.CreateObj(request)).Result.Message;

                if (result.IsSuccess)
                {
                    if (result.Body.ProjectUsers != null && result.Body.ProjectUsers.Count > 0)
                    {
                        cache.AddRange(result.Body.ProjectUsers);
                        response.AddRange(result.Body.ProjectUsers);
                    }

                    _memoryCache.Set(ProjectUserCacheKey, cache.Where(pud => pud.CreatedAtUtc - DateTime.UtcNow < _cacheLifePeriod).ToList());

                    return response;
                }

                _logger.LogWarning(logError + "Errors: {errors}.", string.Join(", ", request), string.Join("\n", result.Errors));
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, logError, string.Join(", ", request));
            }

            errors.Add(messageError);
            return null;
        }

        private List<UserInfo> GetUsersData(List<Guid> userIds, List<string> errors)
        {
            if (userIds == null || !userIds.Any())
            {
                return new();
            }

            string message = "Cannot get users data. Please try again later.";
            string loggerMessage = $"Cannot get users data for specific user ids:'{string.Join(",", userIds)}'.";

            try
            {
                var response = _rcGetUsers.GetResponse<IOperationResult<IGetUsersDataResponse>>(
                    IGetUsersDataRequest.CreateObj(userIds)).Result;

                if (response.Message.IsSuccess)
                {
                    return response.Message.Body.UsersData.Select(_userInfoMapper.Map).ToList();
                }

                _logger.LogWarning(loggerMessage + "Reasons: {Errors}", string.Join("\n", response.Message.Errors));
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, loggerMessage);
            }

            errors.Add(message);

            return new();
        }

        public FindWorkTimesCommand(
            IWorkTimeInfoMapper mapper,
            IWorkTimeRepository repository,
            IWorkTimeMonthLimitRepository monthLimitRepository,
            IAccessValidator accessValidator,
            IHttpContextAccessor httpContextAccessor,
            IRequestClient<IFindProjectsRequest> rcFindProjects,
            IRequestClient<IGetUsersDataRequest> rcGetUsers,
            IRequestClient<IGetProjectUsersRequest> rcGetProjectUsers,
            ILogger<FindWorkTimesCommand> logger,
            IProjectInfoMapper projectInfoMapper,
            IUserInfoMapper userInfoMapper,
            IWorkTimeMonthLimitInfoMapper monthLimitMapper,
            IMemoryCache memoryCache)
        {
            _mapper = mapper;
            _workTimeRepository = repository;
            _monthLimitRepository = monthLimitRepository;
            _accessValidator = accessValidator;
            _httpContextAccessor = httpContextAccessor;
            _rcFindProjects = rcFindProjects;
            _rcGetUsers = rcGetUsers;
            _rcGetProjectUsers = rcGetProjectUsers;
            _logger = logger;
            _projectInfoMapper = projectInfoMapper;
            _userInfoMapper = userInfoMapper;
            _monthLimitMapper = monthLimitMapper;
            _memoryCache = memoryCache;
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

            var dbWorkTimes = _workTimeRepository.Find(filter, skipCount, takeCount, out int totalCount);

            List<ProjectInfo> projects = FindProjects(dbWorkTimes.Select(wt => wt.ProjectId).Distinct().ToList(), errors);
            List<UserInfo> users = GetUsersData(dbWorkTimes.Select(wt => wt.UserId).Distinct().ToList(), errors);
            List<ProjectUserData> projectUsers = GetProjectUsers(dbWorkTimes.Select(wt => (wt.ProjectId, wt.UserId)).ToList(), errors);

            List<WorkTimeMonthLimitInfo> monthLimits = _monthLimitRepository.Find(
                new()
                {
                    Month = filter.Month,
                    Year = filter.Year
                })
                .Select(_monthLimitMapper.Map)
                .ToList();

            return new()
            {
                Status = errors.Any() ? OperationResultStatusType.PartialSuccess : OperationResultStatusType.FullSuccess,
                TotalCount = totalCount,
                Body = dbWorkTimes.Select(
                    wt =>
                        _mapper.Map(
                            wt,
                            users.FirstOrDefault(u => u.Id == wt.UserId),
                            projectUsers.FirstOrDefault(pu => pu.UserId == wt.UserId && pu.ProjectId == wt.ProjectId),
                            projects.FirstOrDefault(p => p.Id == wt.ProjectId),
                            monthLimits.FirstOrDefault(p => p.Year == wt.Year && p.Month == wt.Month)))
                    .ToList(),
                Errors = errors
            };
        }
    }
}
