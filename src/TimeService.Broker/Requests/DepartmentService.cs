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

    public DepartmentService(
      ILogger<DepartmentService> logger,
      IGlobalCacheRepository globalCache,
      IRequestClient<IFilterDepartmentsRequest> rcFilterDepartments,
      IRequestClient<IGetDepartmentUsersRequest> rcGetDepartmentUsers)
    {
      _logger = logger;
      _globalCache = globalCache;
      _rcFilterDepartments = rcFilterDepartments;
      _rcGetDepartmentUsers = rcGetDepartmentUsers;
    }

    public async Task<(List<Guid> usersIds, int totalCount)> GetDepartmentUsersAsync(Guid departmentId, int skipCount, int takeCount, List<string> errors)
    {
      IGetDepartmentUsersResponse response = await RequestHandler.ProcessRequest<IGetDepartmentUsersRequest, IGetDepartmentUsersResponse>(
          _rcGetDepartmentUsers,
          IGetDepartmentUsersRequest.CreateObj(
            departmentId,
            skipCount: skipCount,
            takeCount: takeCount),
          errors,
          _logger);

      if (response is null)
      {
        errors.Add("Can not get department users data.");

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

      if (departmentsData is null)
      {
        errors.Add("Can not get departments data.");
      }

      return departmentsData;
    }
  }
}
