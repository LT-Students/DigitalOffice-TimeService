using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.Helpers;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Extensions;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.Models.Broker.Requests.Department;
using LT.DigitalOffice.Models.Broker.Responses.Department;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Broker.Requests
{
  public class DepartmentService : IDepartmentService
  {
    private readonly ILogger<DepartmentService> _logger;
    private readonly IGlobalCacheRepository _globalCache;
    private readonly IRequestClient<IFilterDepartmentsRequest> _rcFilterDepartments;
    private readonly IRequestClient<IGetDepartmentsUsersRequest> _rcGetDepartmentUsers;
    private readonly IRequestClient<IGetDepartmentsRequest> _rcGetDepartments;
    private readonly IRequestClient<IGetDepartmentUserRoleRequest> _rcGetDepartmentUserRole;

    private List<Guid> GetRedisKeyArray(List<Guid> departmentsIds = null, List<Guid> usersIds = null)
    {
      List<Guid> keyAray = new List<Guid>();

      if (departmentsIds is not null)
      {
        keyAray.AddRange(departmentsIds);
      }

      if (usersIds is not null)
      {
        keyAray.AddRange(usersIds);
      }

      return keyAray;
    }

    public DepartmentService(
      ILogger<DepartmentService> logger,
      IGlobalCacheRepository globalCache,
      IRequestClient<IFilterDepartmentsRequest> rcFilterDepartments,
      IRequestClient<IGetDepartmentsUsersRequest> rcGetDepartmentUsers,
      IRequestClient<IGetDepartmentsRequest> rcGetDepartments,
      IRequestClient<IGetDepartmentUserRoleRequest> rcGetDepartmentUserRole)
    {
      _logger = logger;
      _globalCache = globalCache;
      _rcFilterDepartments = rcFilterDepartments;
      _rcGetDepartmentUsers = rcGetDepartmentUsers;
      _rcGetDepartments = rcGetDepartments;
      _rcGetDepartmentUserRole = rcGetDepartmentUserRole;
    }

    public async Task<List<DepartmentUserExtendedData>> GetDepartmentsUsersAsync(
      List<Guid> departmentsIds,
      DateTime? byEntryDate = null,
      bool includePendingUsers = false,
      List<string> errors = null)
    {
      IGetDepartmentsUsersResponse response = await _rcGetDepartmentUsers
        .ProcessRequest<IGetDepartmentsUsersRequest, IGetDepartmentsUsersResponse>(
          IGetDepartmentsUsersRequest.CreateObj(
            departmentsIds: departmentsIds,
            byEntryDate: byEntryDate,
            includePendingUsers: includePendingUsers),
          errors,
          _logger);

      if (response is null)
      {
        return default;
      }

      return response.Users;
    }

    public async Task<List<DepartmentFilteredData>> GetDepartmentFilteredDataAsync(List<Guid> departmentsIds, List<string> errors = null)
    {
      if (departmentsIds is null || !departmentsIds.Any())
      {
        return null;
      }

      object request = IFilterDepartmentsRequest.CreateObj(departmentsIds);

      List<DepartmentFilteredData> departmentsData =
        await _globalCache.GetAsync<List<DepartmentFilteredData>>(Cache.Departments, departmentsIds.GetRedisCacheKey(
          nameof(IFilterDepartmentsRequest), request.GetBasicProperties()));

      if (departmentsData is null)
      {
        departmentsData =
          (await _rcFilterDepartments.ProcessRequest<IFilterDepartmentsRequest, IFilterDepartmentsResponse>(
            request,
            errors,
            _logger))
          ?.Departments;
      }

      return departmentsData;
    }

    public async Task<List<DepartmentData>> GetDepartmentsDataAsync(
      List<Guid> departmentsIds = null,
      List<Guid> usersIds = null,
      List<string> errors = null)
    {
      object request = IGetDepartmentsRequest.CreateObj(
        departmentsIds: departmentsIds,
        usersIds: usersIds);

      List<DepartmentData> departmentsData = await _globalCache.GetAsync<List<DepartmentData>>(
        Cache.Departments, GetRedisKeyArray(departmentsIds, usersIds).GetRedisCacheKey(
          nameof(IGetDepartmentsRequest), request.GetBasicProperties()));

      if (departmentsData is null)
      {
        departmentsData =
          (await _rcGetDepartments.ProcessRequest<IGetDepartmentsRequest, IGetDepartmentsResponse>(
            request,
            errors,
            _logger))
          ?.Departments;
      }

      return departmentsData;
    }

    public async Task<DepartmentUserRole?> GetDepartmentUserRoleAsync(
      Guid departmentId,
      Guid userId,
      List<string> errors = null)
    {
      IGetDepartmentUserRoleResponse response = await _rcGetDepartmentUserRole.ProcessRequest<IGetDepartmentUserRoleRequest, IGetDepartmentUserRoleResponse>(
        IGetDepartmentUserRoleRequest.CreateObj(
          departmentId: departmentId,
          userId: userId),
        errors, _logger);

      return response?.DepartmentUserRole;
    }
  }
}
