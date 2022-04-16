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

    public async Task<List<ProjectData>> GetProjectsDataAsync(List<Guid> projectsIds, List<string> errors)
    {
      if (projectsIds is null || !projectsIds.Any())
      {
        return null;
      }

      List<ProjectData> projectsData;

      (projectsData, int _) = await _globalCache.GetAsync<(List<ProjectData>, int)>(Cache.Projects, projectsIds.GetRedisCacheHashCode(true));

      if (projectsData is null)
      {
        projectsData =
          (await RequestHandler.ProcessRequest<IGetProjectsRequest, IGetProjectsResponse>(
            _rcGetProjects,
            IGetProjectsRequest.CreateObj(projectsIds, includeUsers: true),
            errors,
            _logger))
          ?.Projects;
      }

      if (projectsData is null)
      {
        errors.Add("Can not get projects data.");
      }

      return projectsData;
    }

    public async Task<(List<ProjectUserData> projectUsersData, int totalCount)> GetProjectUsersAsync(Guid projectId, int skipCount, int takeCount, List<string> errors)
    {
      IGetProjectsUsersResponse response =
          (await RequestHandler.ProcessRequest<IGetProjectsUsersRequest, IGetProjectsUsersResponse>(
            _rcGetProjectsUsers,
            IGetProjectsUsersRequest.CreateObj(
              projectsIds: new() { projectId },
              skipCount: skipCount,
              takeCount: takeCount),
            errors,
            _logger));

      if (response is null)
      {
        errors.Add("Can not get project users data.");

        return default;
      }

      return (response.Users, response.TotalCount);
    }
  }
}
