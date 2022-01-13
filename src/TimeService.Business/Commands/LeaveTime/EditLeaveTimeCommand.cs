using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Patch.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
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

    private async Task<bool> ValidateOverlappingAsync(
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

      DateTimeOffset start;
      DateTimeOffset end;

      if (startTimeOperation is null)
      {
        end = DateTimeOffset.Parse(endTimeOperation.value.ToString());
        start = new DateTimeOffset(DateTime.SpecifyKind(oldLeaveTime.StartTime, DateTimeKind.Unspecified), end.Offset);
      }
      else
      {
        start = DateTimeOffset.Parse(startTimeOperation.value.ToString());
        end = endTimeOperation is null
          ? new DateTimeOffset(DateTime.SpecifyKind(oldLeaveTime.EndTime, DateTimeKind.Unspecified), start.Offset)
          : DateTimeOffset.Parse(endTimeOperation.value.ToString());
      }

      if (start >= end)
      {
        errors.Add("Start time must be less than end time.");

        return false;
      }

      if (await _repository.HasOverlapAsync(oldLeaveTime, start.UtcDateTime, end.UtcDateTime))
      {
        errors.Add("Incorrect time interval.");

        return false;
      }

      DateTime createdAt = oldLeaveTime.CreatedAtUtc.Add(start.Offset);

      switch (oldLeaveTime.LeaveType)
      {
        case (int)LeaveType.SickLeave:
          if (start.DateTime < createdAt.AddMonths(-1) || end.DateTime > createdAt.AddMonths(1))
          {
            errors.Add("Incorrect interval for leave time.");

            return false;
          }
          break;

        default:
          if (start.DateTime < createdAt.AddMonths(-1)
            || (start.Month == createdAt.AddMonths(-1).Month && createdAt.Day > 5))
          {
            errors.Add("Incorrect interval for leave time.");

            return false;
          }
          break;
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

    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid leaveTimeId, JsonPatchDocument<EditLeaveTimeRequest> request)
    {
      DbLeaveTime oldLeaveTime = await _repository.GetAsync(leaveTimeId);

      if (_httpContextAccessor.HttpContext.GetUserId() != oldLeaveTime.UserId
        && !await _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime))
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

        return new OperationResultResponse<bool>
        {
          Status = OperationResultStatusType.Failed,
          Errors = new() { "Not enough rights." }
        };
      }

      if (!_validator.ValidateCustom(request, out List<string> errors))
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return new OperationResultResponse<bool>
        {
          Status = OperationResultStatusType.Failed,
          Errors = errors
        };
      }

      if (!await ValidateOverlappingAsync(oldLeaveTime, request, errors))
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
        Body = await _repository.EditAsync(oldLeaveTime, _mapper.Map(request)),
        Status = OperationResultStatusType.FullSuccess,
        Errors = errors
      };
    }
  }
}
