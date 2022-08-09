using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.Helpers;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Extensions;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
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
    private readonly IRequestClient<IGetDepartmentUsersRequest> _rcGetDepartmentUsers;
    private readonly IRequestClient<IGetDepartmentsRequest> _rcGetDepartments;

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
      IRequestClient<IGetDepartmentUsersRequest> rcGetDepartmentUsers,
      IRequestClient<IGetDepartmentsRequest> rcGetDepartments)
    {
      _logger = logger;
      _globalCache = globalCache;
      _rcFilterDepartments = rcFilterDepartments;
      _rcGetDepartmentUsers = rcGetDepartmentUsers;
      _rcGetDepartments = rcGetDepartments;
    }

    public async Task<(List<Guid> usersIds, int totalCount)> GetDepartmentUsersAsync(
      Guid departmentId,
      List<string> errors,
      int? skipCount = null,
      int? takeCount = null,
      DateTime? byEntryDate = null)
    {
      IGetDepartmentUsersResponse response = await RequestHandler.ProcessRequest<IGetDepartmentUsersRequest, IGetDepartmentUsersResponse>(
          _rcGetDepartmentUsers,
          IGetDepartmentUsersRequest.CreateObj(
            departmentId,
            skipCount: skipCount,
            takeCount: takeCount,
            ByEntryDate: byEntryDate),
          errors,
          _logger);

      if (response is null)
      {
        return default;
      }

      return (response.UsersIds, response.TotalCount);
    }

    public async Task<List<DepartmentFilteredData>> GetDepartmentFilteredDataAsync(List<Guid> departmentsIds, List<string> errors)
    {
      if (departmentsIds is null || !departmentsIds.Any())
      {
        return null;
      }

      List<DepartmentFilteredData> departmentsData = await _globalCache.GetAsync<List<DepartmentFilteredData>>(Cache.Departments, departmentsIds.GetRedisCacheHashCode());

      if (departmentsData is null)
      {
        departmentsData =
          (await RequestHandler.ProcessRequest<IFilterDepartmentsRequest, IFilterDepartmentsResponse>(
            _rcFilterDepartments,
            IFilterDepartmentsRequest.CreateObj(departmentsIds),
            errors,
            _logger))
          ?.Departments;
      }

      return departmentsData;
    }

    public async Task<List<DepartmentData>> GetDepartmentsDataAsync(
      List<string> errors,
      List<Guid> departmentsIds = null,
      List<Guid> usersIds = null)
    {
      List<DepartmentData> departmentsData = await _globalCache.GetAsync<List<DepartmentData>>(
        Cache.Departments, GetRedisKeyArray(departmentsIds, usersIds).GetRedisCacheHashCode());

      if (departmentsData is null)
      {
        departmentsData =
          (await RequestHandler.ProcessRequest<IGetDepartmentsRequest, IGetDepartmentsResponse>(
            _rcGetDepartments,
            IGetDepartmentsRequest.CreateObj(
              departmentsIds: departmentsIds,
              usersIds: usersIds),
            errors,
            _logger))
          ?.Departments;
      }

      return departmentsData;
    }
  }
}
