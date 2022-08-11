﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.Helpers;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Extensions;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Enums;
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
    private readonly IRequestClient<IGetProjectUserRoleRequest> _rcGetProjectUserRole;

    private string CreateGetProjectCacheKey(
      List<Guid> projectIds = null,
      List<Guid> usersIds = null,
      bool includeUsers = false,
      bool includeDepartment = false)
    {
      List<Guid> ids = new();

      if (projectIds is not null && projectIds.Any())
      {
        ids.AddRange(projectIds);
      }

      if (usersIds is not null && usersIds.Any())
      {
        ids.AddRange(usersIds);
      }

      List<object> args = new() { includeDepartment, includeUsers };

      return ids.GetRedisCacheHashCode(args.ToArray());
    }

    public ProjectService(
      ILogger<ProjectService> logger,
      IGlobalCacheRepository globalCache,
      IRequestClient<IGetProjectsRequest> rcGetProjects,
      IRequestClient<IGetProjectsUsersRequest> rcGetProjectsUsers,
      IRequestClient<IGetProjectUserRoleRequest> rcGetProjectUserRole)
    {
      _logger = logger;
      _globalCache = globalCache;
      _rcGetProjects = rcGetProjects;
      _rcGetProjectsUsers = rcGetProjectsUsers;
      _rcGetProjectUserRole = rcGetProjectUserRole;
    }

    public async Task<List<ProjectData>> GetProjectsDataAsync(
      List<Guid> projectsIds = null,
      List<Guid> usersIds = null,
      bool includeUsers = false,
      bool includeDepartments = false,
      List<string> errors = null)
    {
      if ((projectsIds is null || !projectsIds.Any()) && (usersIds is null || !usersIds.Any()))
      {
        return null;
      }

      List<ProjectData> projectsData;

      (projectsData, int _) = await _globalCache.GetAsync<(List<ProjectData>, int)>(
        Cache.Projects, CreateGetProjectCacheKey(projectsIds, usersIds, includeUsers, includeDepartments));

      if (projectsData is null)
      {
        projectsData =
          (await _rcGetProjects.ProcessRequest<IGetProjectsRequest, IGetProjectsResponse>(
            IGetProjectsRequest.CreateObj(
              projectsIds: projectsIds,
              usersIds: usersIds,
              includeUsers: includeUsers,
              includeDepartment: includeDepartments),
            errors,
            _logger))
          ?.Projects;
      }

      return projectsData;
    }

    public async Task<List<ProjectUserData>> GetProjectsUsersAsync(
      List<Guid> projectsIds = null,
      List<Guid> usersIds = null,
      DateTime? byEntryDate = null,
      List<string> errors = null)
    {
      IGetProjectsUsersResponse response =
          (await _rcGetProjectsUsers.ProcessRequest<IGetProjectsUsersRequest, IGetProjectsUsersResponse>(
            IGetProjectsUsersRequest.CreateObj(
              projectsIds: projectsIds,
              usersIds: usersIds,
              byEntryDate: byEntryDate),
            errors,
            _logger));

      return response?.Users;
    }

    public async Task<ProjectUserRoleType?> GetProjectUserRoleAsync(
      Guid userId,
      Guid projectId,
      List<string> errors = null)
    {
      IGetProjectUserRoleResponse response = await _rcGetProjectUserRole.ProcessRequest<IGetProjectUserRoleRequest, IGetProjectUserRoleResponse>(
        IGetProjectUserRoleRequest.CreateObj(
          userId: userId,
          projectId: projectId),
        errors);

      return response?.ProjectUserRole;
    }
  }
}
