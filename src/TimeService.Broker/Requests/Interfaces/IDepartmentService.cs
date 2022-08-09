using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models.Department;

namespace LT.DigitalOffice.TimeService.Broker.Requests.Interfaces
{
  [AutoInject]
  public interface IDepartmentService
  {
    Task<(List<Guid> usersIds, int totalCount)> GetDepartmentUsersAsync(
      Guid departmentId,
      List<string> errors,
      int? skipCount = null,
      int? takeCount = null,
      DateTime? byEntryDate = null);

    Task<List<DepartmentData>> GetDepartmentsDataAsync(
      List<string> errors,
      List<Guid> departmentsIds = null,
      List<Guid> usersIds = null);

    Task<List<DepartmentFilteredData>> GetDepartmentFilteredDataAsync(List<Guid> departmentsIds, List<string> errors);
  }
}
