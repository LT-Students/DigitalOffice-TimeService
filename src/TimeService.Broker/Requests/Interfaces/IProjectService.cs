using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models.Project;

namespace LT.DigitalOffice.TimeService.Broker.Requests.Interfaces
{
  [AutoInject]
  public interface IProjectService
  {
    Task<List<ProjectData>> GetProjectsDataAsync(
      List<string> errors,
      List<Guid> projectsIds = null,
      Guid? userId = null,
      bool includeUsers = false);

    Task<(List<ProjectUserData> projectUsersData, int totalCount)> GetProjectUsersAsync(
      List<string> errors,
      List<Guid> projectsIds = null,
      List<Guid> usersIds = null,
      int? skipCount = null,
      int? takeCount = null);
  }
}
