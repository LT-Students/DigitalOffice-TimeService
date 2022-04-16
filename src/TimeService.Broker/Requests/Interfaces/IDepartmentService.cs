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
    Task<(List<Guid> usersIds, int totalCount)> GetDepartmentUsersAsync(Guid departmentId, int skipCount, int takeCount, List<string> errors);
    Task<List<DepartmentFilteredData>> GetDepartmentFilteredDataAsync(List<Guid> departmentsIds, List<string> errors);
  }
}
