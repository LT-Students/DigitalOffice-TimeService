using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models.Department;

namespace LT.DigitalOffice.TimeService.Broker.Requests.Interfaces
{
  [AutoInject]
  public interface IDepartmentService
  {
    Task<List<DepartmentUserExtendedData>> GetDepartmentsUsersAsync(
      List<Guid> departmentsIds,
      DateTime? byEntryDate = null,
      bool includePendingUsers = false,
      List<string> errors = null);

    Task<List<DepartmentData>> GetDepartmentsDataAsync(
      List<Guid> departmentsIds = null,
      List<Guid> usersIds = null,
      List<string> errors = null);

    Task<List<DepartmentFilteredData>> GetDepartmentFilteredDataAsync(List<Guid> departmentsIds, List<string> errors = null);

    Task<DepartmentUserRole?> GetDepartmentUserRoleAsync(
      Guid departmentId,
      Guid userId,
      List<string> errors = null);
  }
}
