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
    Task<List<ProjectData>> GetProjectsDataAsync(List<Guid> projectsIds, List<string> errors);
    Task<(List<ProjectUserData> projectUsersData, int totalCount)> GetProjectUsersAsync(Guid projectId, int skipCount, int takeCount, List<string> errors);
  }
}
