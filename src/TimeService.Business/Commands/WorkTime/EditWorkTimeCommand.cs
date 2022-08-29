using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Patch.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime
{
  public class EditWorkTimeCommand : IEditWorkTimeCommand
  {
    private readonly IEditWorkTimeRequestValidator _validator;
    private readonly IWorkTimeRepository _repository;
    private readonly IPatchDbWorkTimeMapper _patchMapper;
    private readonly IDbWorkTimeMapper _dbMapper;
    private readonly IProjectService _projectService;
    private readonly IDepartmentService _departmentService;
    private readonly IAccessValidator _accessValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IResponseCreator _responseCreator;

    public EditWorkTimeCommand(
      IEditWorkTimeRequestValidator validator,
      IWorkTimeRepository repository,
      IPatchDbWorkTimeMapper patchMapper,
      IDbWorkTimeMapper dbMapper,
      IProjectService projectService,
      IDepartmentService departmentService,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor,
      IResponseCreator responseCreator)
    {
      _validator = validator;
      _repository = repository;
      _patchMapper = patchMapper;
      _dbMapper = dbMapper;
      _projectService = projectService;
      _departmentService = departmentService;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
      _responseCreator = responseCreator;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid workTimeId, JsonPatchDocument<EditWorkTimeRequest> request)
    {
      DbWorkTime oldDbWorkTime = await _repository.GetAsync(workTimeId);

      Guid senderId = _httpContextAccessor.HttpContext.GetUserId();
      bool isOwner = senderId == oldDbWorkTime.UserId;

      if (!isOwner)
      {
        Task<List<DepartmentData>> getWtOwnerDepartmentTask = _departmentService.GetDepartmentsDataAsync(usersIds: new() { oldDbWorkTime.UserId });
        Task<ProjectUserRoleType?> senderProjectRoleTask = _projectService.GetProjectUserRoleAsync(userId: senderId, projectId: oldDbWorkTime.ProjectId);
        Task<bool> hasRightsTask = _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime);

        Task<DepartmentUserRole?> senderDepartmentRoleTask = (await getWtOwnerDepartmentTask)?.FirstOrDefault() != null
        ? _departmentService.GetDepartmentUserRoleAsync(userId: senderId, departmentId: (await getWtOwnerDepartmentTask).First().Id)
        : Task.FromResult(default(DepartmentUserRole?));

        if (await senderProjectRoleTask != ProjectUserRoleType.Manager
          && await senderDepartmentRoleTask != DepartmentUserRole.Manager
          && !await hasRightsTask)
        {
          return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
        }
      }

      if (!_validator.ValidateCustom((oldDbWorkTime.ProjectId, request), out List<string> errors))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.BadRequest, errors);
      }

      OperationResultResponse<bool> response = new();

      if (!isOwner)
      {
        if (oldDbWorkTime.ParentId is null && oldDbWorkTime.ManagerWorkTime is null)
        {
          DbWorkTime managerWorkTime = _dbMapper.Map(oldDbWorkTime, senderId);

          _patchMapper.Map(request).ApplyTo(managerWorkTime);

          bool result = (await _repository.CreateAsync(managerWorkTime)).HasValue;

          response.Body = result;
        }
        else
        {
          response.Body = await _repository.EditAsync(oldDbWorkTime.ManagerWorkTime ?? oldDbWorkTime, _patchMapper.Map(request));
        }
      }
      else
      {
        response.Body = await _repository.EditAsync(oldDbWorkTime, _patchMapper.Map(request));
      }

      return response;
    }
  }
}
