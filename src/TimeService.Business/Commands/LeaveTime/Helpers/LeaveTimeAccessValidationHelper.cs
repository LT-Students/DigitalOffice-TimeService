using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Helpers
{
  public class LeaveTimeAccessValidationHelper : ILeaveTimeAccessValidationHelper
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDepartmentService _departmentService;
    private readonly IProjectService _projectService;
    private readonly IAccessValidator _accessValidator;

    public LeaveTimeAccessValidationHelper(
      IHttpContextAccessor httpContextAccessor,
      IDepartmentService departmentService,
      IProjectService projectService,
      IAccessValidator accessValidator)
    {
      _httpContextAccessor = httpContextAccessor;
      _departmentService = departmentService;
      _projectService = projectService;
      _accessValidator = accessValidator;
    }

    public async Task<bool> HasRightsAsync(Guid ltOwnerId)
    {
      Guid senderId = _httpContextAccessor.HttpContext.GetUserId();

      Task<List<DepartmentData>> leaveTimeOwnerDepartmentTask = _departmentService.GetDepartmentsDataAsync(usersIds: new() { ltOwnerId });
      Task<List<ProjectUserData>> projectsUsersTask = _projectService.GetProjectsUsersAsync(usersIds: new() { senderId, ltOwnerId });

      Task<bool> hasRightsTask = _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime);

      Task<DepartmentUserRole?> senderDepartmentRoleTask = (await leaveTimeOwnerDepartmentTask)?.FirstOrDefault() is not null
        ? _departmentService.GetDepartmentUserRoleAsync(userId: senderId, departmentId: (await leaveTimeOwnerDepartmentTask).First().Id)
        : Task.FromResult(default(DepartmentUserRole?));

      bool isSenderProjectManager = (await projectsUsersTask)?.Where(x => x.UserId == senderId)
        .IntersectBy((await projectsUsersTask)?.Where(x => x.UserId == ltOwnerId).Select(x => x.ProjectId), x => x.ProjectId)?
        .Any(x => x.ProjectUserRole == ProjectUserRoleType.Manager && x.UserId == senderId) ?? false;

      return isSenderProjectManager || await hasRightsTask || (await senderDepartmentRoleTask) == DepartmentUserRole.Manager;
    }
  }
}
