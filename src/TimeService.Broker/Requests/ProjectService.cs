using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.Helpers;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Extensions;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Broker.Requests
{
  public class ProjectService : IProjectService
  {
    private readonly ILogger<ProjectService> _logger;
    private readonly IGlobalCacheRepository _globalCache;
    private readonly IRequestClient<IGetProjectsRequest> _rcGetProjects;
    private readonly IRequestClient<IGetProjectsUsersRequest> _rcGetProjectsUsers;

    private string CreateGetProjectCacheKey(
      List<Guid> projectIds = null,
      Guid? userId = null,
      Guid? departmentId = null,
      bool includeUsers = false)
    {
      List<Guid> ids = new();

      if (projectIds is not null && projectIds.Any())
      {
        ids.AddRange(projectIds);
      }

      if (userId.HasValue)
      {
        ids.Add(userId.Value);
      }

      if (departmentId.HasValue)
      {
        ids.Add(departmentId.Value);
      }

      return ids.GetRedisCacheHashCode(includeUsers);
    }

    public ProjectService(
      ILogger<ProjectService> logger,
      IGlobalCacheRepository globalCache,
      IRequestClient<IGetProjectsRequest> rcGetProjects,
      IRequestClient<IGetProjectsUsersRequest> rcGetProjectsUsers)
    {
      _logger = logger;
      _globalCache = globalCache;
      _rcGetProjects = rcGetProjects;
      _rcGetProjectsUsers = rcGetProjectsUsers;
    }

    public async Task<List<ProjectData>> GetProjectsDataAsync(
      List<string> errors,
      List<Guid> projectsIds = null,
      Guid? departmentId = null,
      Guid? userId = null,
      bool includeUsers = false)
    {
      if (projectsIds is null || !projectsIds.Any())
      {
        return null;
      }

      List<ProjectData> projectsData;

      (projectsData, int _) = await _globalCache.GetAsync<(List<ProjectData>, int)>(
        Cache.Projects, CreateGetProjectCacheKey(projectsIds, userId, departmentId, includeUsers));

      if (projectsData is null)
      {
        projectsData =
          (await RequestHandler.ProcessRequest<IGetProjectsRequest, IGetProjectsResponse>(
            _rcGetProjects,
            IGetProjectsRequest.CreateObj(
              projectsIds: projectsIds,
              userId: userId,
              includeUsers: includeUsers),
            errors,
            _logger))
          ?.Projects;
      }

      return projectsData;
    }

    public async Task<(List<ProjectUserData> projectUsersData, int totalCount)> GetProjectUsersAsync(
      List<string> errors,
      List<Guid> projectsIds = null,
      List<Guid> usersIds = null,
      int? skipCount = null,
      int? takeCount = null)
    {
      IGetProjectsUsersResponse response =
          (await RequestHandler.ProcessRequest<IGetProjectsUsersRequest, IGetProjectsUsersResponse>(
            _rcGetProjectsUsers,
            IGetProjectsUsersRequest.CreateObj(
              projectsIds: projectsIds,
              usersIds: usersIds,
              skipCount: skipCount,
              takeCount: takeCount),
            errors,
            _logger));

      if (response is null)
      {
        return default;
      }

      return (response.Users, response.TotalCount);
    }
  }
}
