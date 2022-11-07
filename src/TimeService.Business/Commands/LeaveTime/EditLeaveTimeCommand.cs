using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Helpers;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Patch.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime
{
  public class EditLeaveTimeCommand : IEditLeaveTimeCommand
  {
    private readonly IEditLeaveTimeRequestValidator _validator;
    private readonly ILeaveTimeRepository _repository;
    private readonly IPatchDbLeaveTimeMapper _patchDbLeaveTimeapper;
    private readonly IDbLeaveTimeMapper _dbLeaveTimeMapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IResponseCreator _responseCreator;
    private readonly ILeaveTimeAccessValidationHelper _ltAccessValidationHelper;

    public EditLeaveTimeCommand(
      IEditLeaveTimeRequestValidator validator,
      ILeaveTimeRepository repository,
      IPatchDbLeaveTimeMapper patchDbLeaveTimeapper,
      IDbLeaveTimeMapper dbLeaveTimeMapper,
      IHttpContextAccessor httpContextAccessor,
      IResponseCreator responseCreator,
      ILeaveTimeAccessValidationHelper ltAccessValidationHelper)
    {
      _validator = validator;
      _repository = repository;
      _patchDbLeaveTimeapper = patchDbLeaveTimeapper;
      _dbLeaveTimeMapper = dbLeaveTimeMapper;
      _httpContextAccessor = httpContextAccessor;
      _responseCreator = responseCreator;
      _ltAccessValidationHelper = ltAccessValidationHelper;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid leaveTimeId, JsonPatchDocument<EditLeaveTimeRequest> request)
    {
      DbLeaveTime oldLeaveTime = await _repository.GetAsync(leaveTimeId);
      bool isOwner = _httpContextAccessor.HttpContext.GetUserId() == oldLeaveTime.UserId;

      if (!isOwner && !await _ltAccessValidationHelper.HasRightsAsync(ltOwnerId: oldLeaveTime.UserId))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      ValidationResult validationResult = await _validator.ValidateAsync((oldLeaveTime, request));
      if (!validationResult.IsValid)
      {
        return _responseCreator.CreateFailureResponse<bool>(
          HttpStatusCode.BadRequest,
          validationResult.Errors?.Select(vf => vf.ErrorMessage).ToList());
      }

      OperationResultResponse<bool> response = new OperationResultResponse<bool>();

      if (!isOwner)
      {
        // checks that user's (not manager's) leave time is edited
        if (oldLeaveTime.ParentId is null && oldLeaveTime.ManagerLeaveTime is null)
        {
          DbLeaveTime managerLeaveTime = _dbLeaveTimeMapper.Map(oldLeaveTime, _httpContextAccessor.HttpContext.GetUserId());

          _patchDbLeaveTimeapper.Map(request).ApplyTo(managerLeaveTime);

          await _repository.CreateAsync(managerLeaveTime);
        }
        else
        {
          response.Body = await _repository.EditAsync(oldLeaveTime.ManagerLeaveTime ?? oldLeaveTime, _patchDbLeaveTimeapper.Map(request));
        }
      }
      else
      {
        response.Body = await _repository.EditAsync(oldLeaveTime, _patchDbLeaveTimeapper.Map(request));
      }

      return response;
    }
  }
}
