﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Patch.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime
{
  public class EditLeaveTimeCommand : IEditLeaveTimeCommand
  {
    private readonly IEditLeaveTimeRequestValidator _validator;
    private readonly ILeaveTimeRepository _repository;
    private readonly IPatchDbLeaveTimeMapper _mapper;
    private readonly IAccessValidator _accessValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private bool ValidateOverlapping(
      DbLeaveTime oldLeaveTime, 
      JsonPatchDocument<EditLeaveTimeRequest> request,
      List<string> errors)
    {
      Operation<EditLeaveTimeRequest> startTimeOperation = request.Operations.FirstOrDefault(
        o => o.path.EndsWith(nameof(EditLeaveTimeRequest.StartTime), StringComparison.OrdinalIgnoreCase));
      Operation<EditLeaveTimeRequest> endTimeOperation = request.Operations.FirstOrDefault(
        o => o.path.EndsWith(nameof(EditLeaveTimeRequest.EndTime), StringComparison.OrdinalIgnoreCase));

      if (startTimeOperation == null && endTimeOperation == null)
      {
        return true;
      }

      DateTime? start = startTimeOperation == null ? null : DateTime.Parse(startTimeOperation.value.ToString());
      DateTime? end = endTimeOperation == null ? null : DateTime.Parse(endTimeOperation.value.ToString());

      const string compareTimeError = "Start time must be less than end time.";
      const string intervalError = "Incorrect time interval.";

      if (start.HasValue && !end.HasValue && oldLeaveTime.EndTime <= start
        || !start.HasValue && end.HasValue && oldLeaveTime.StartTime >= end)
      {
        errors.Add(compareTimeError);

        return false;
      }

      if (_repository.HasOverlap(oldLeaveTime, start, end))
      {
        errors.Add(intervalError);

        return false;
      }

      return true;
    }

    public EditLeaveTimeCommand(
      IEditLeaveTimeRequestValidator validator,
      ILeaveTimeRepository repository,
      IPatchDbLeaveTimeMapper mapper,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor)
    {
      _validator = validator;
      _repository = repository;
      _mapper = mapper;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
    }

    public OperationResultResponse<bool> Execute(Guid leaveTimeId, JsonPatchDocument<EditLeaveTimeRequest> request)
    {
      var oldLeaveTime = _repository.Get(leaveTimeId);

      var isOwner = _httpContextAccessor.HttpContext.GetUserId() == oldLeaveTime.UserId;
      if (!isOwner && !_accessValidator.IsAdmin())
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

        return new OperationResultResponse<bool>
        {
          Status = OperationResultStatusType.Failed,
          Errors = new() { "Not enough rights." }
        };
      }

      if(!_validator.ValidateCustom(request, out List<string> errors))
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return new OperationResultResponse<bool>
        {
          Status = OperationResultStatusType.Failed,
          Errors = errors
        };
      }

      bool isSuccess = ValidateOverlapping(oldLeaveTime, request, errors);

      if (!isSuccess)
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return new OperationResultResponse<bool>
        {
          Status = OperationResultStatusType.Failed,
          Errors = errors
        };
      }

      return new OperationResultResponse<bool>
      {
        Body = _repository.Edit(oldLeaveTime, _mapper.Map(request)),
        Status = OperationResultStatusType.FullSuccess,
        Errors = errors
      };
    }
  }
}
