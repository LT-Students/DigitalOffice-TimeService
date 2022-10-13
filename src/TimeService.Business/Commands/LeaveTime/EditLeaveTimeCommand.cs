using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Helpers;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
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
    private readonly IPatchDbLeaveTimeMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IResponseCreator _responseCreator;
    private readonly ILeaveTimeAccessValidationHelper _ltAccessValidationHelper;

    public EditLeaveTimeCommand(
      IEditLeaveTimeRequestValidator validator,
      ILeaveTimeRepository repository,
      IPatchDbLeaveTimeMapper mapper,
      IHttpContextAccessor httpContextAccessor,
      IResponseCreator responseCreator,
      ILeaveTimeAccessValidationHelper ltAccessValidationHelper)
    {
      _validator = validator;
      _repository = repository;
      _mapper = mapper;
      _httpContextAccessor = httpContextAccessor;
      _responseCreator = responseCreator;
      _ltAccessValidationHelper = ltAccessValidationHelper;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid leaveTimeId, JsonPatchDocument<EditLeaveTimeRequest> request)
    {
      DbLeaveTime oldLeaveTime = await _repository.GetAsync(leaveTimeId);
      bool isOwner = _httpContextAccessor.HttpContext.GetUserId() == oldLeaveTime.UserId;

      if (!isOwner && await _ltAccessValidationHelper.HasRightsAsync(ltOwnerId: oldLeaveTime.UserId))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      if (!_validator.ValidateCustom((oldLeaveTime, request), out List<string> errors))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.BadRequest, errors);
      }

      return new()
      {
        Body = await _repository.EditAsync(oldLeaveTime, _mapper.Map(request)),
        Errors = errors
      };
    }
  }
}
