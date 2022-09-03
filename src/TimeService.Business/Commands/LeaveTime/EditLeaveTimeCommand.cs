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
    private readonly IResponseCreator _responseCreator;

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
        start = new DateTimeOffset(DateTime.SpecifyKind(oldLeaveTime.StartTime, DateTimeKind.Utc), end.Offset);
      }
      else
      {
        start = DateTimeOffset.Parse(startTimeOperation.value.ToString());
        end = endTimeOperation is null
          ? new DateTimeOffset(DateTime.SpecifyKind(oldLeaveTime.EndTime, DateTimeKind.Utc), start.Offset)
          : DateTimeOffset.Parse(endTimeOperation.value.ToString());
      }

      if (start >= end)
      {
        errors.Add("Start time must be less than end time.");

        return false;
      }

      DateTime timeNow = DateTime.UtcNow.Add(start.Offset);

      DateTime thisMonthFirstDay = new DateTime(timeNow.Year, timeNow.Month, 1);
      DateTime startMonthFirstDay = new DateTime(start.Year, start.Month, 1);
      DateTime endMonthFirstDay = new DateTime(end.Year, end.Month, 1);

      bool isEditingStartTimeValid = startTimeOperation is null || (startMonthFirstDay == thisMonthFirstDay.AddMonths(-1) && timeNow.Day <= 5)
        || startMonthFirstDay == thisMonthFirstDay || startMonthFirstDay == thisMonthFirstDay.AddMonths(1);

      bool isEditingEndTimeValid = endTimeOperation is null || (endMonthFirstDay == thisMonthFirstDay.AddMonths(-1) && timeNow.Day <= 5)
        || endMonthFirstDay == thisMonthFirstDay || endMonthFirstDay == thisMonthFirstDay.AddMonths(1);

      if (!isEditingStartTimeValid || !isEditingEndTimeValid)
      {
        errors.Add("Incorrect interval for leave time.");

        return false;
      }

      if (await _repository.HasOverlapAsync(oldLeaveTime, start.UtcDateTime, end.UtcDateTime))
      {
        errors.Add("New LeaveTime should not overlap with old ones.");

        return false;
      }

      return true;
    }

    public EditLeaveTimeCommand(
      IEditLeaveTimeRequestValidator validator,
      ILeaveTimeRepository repository,
      IPatchDbLeaveTimeMapper mapper,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor,
      IResponseCreator responseCreator)
    {
      _validator = validator;
      _repository = repository;
      _mapper = mapper;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
      _responseCreator = responseCreator;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid leaveTimeId, JsonPatchDocument<EditLeaveTimeRequest> request)
    {
      DbLeaveTime oldLeaveTime = await _repository.GetAsync(leaveTimeId);

      if (_httpContextAccessor.HttpContext.GetUserId() != oldLeaveTime.UserId
        && !await _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      if (!_validator.ValidateCustom((oldLeaveTime, request), out List<string> errors))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.BadRequest, errors);
      }/*

      if (!await ValidateOverlappingAsync(oldLeaveTime, request, errors))
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return new OperationResultResponse<bool>
        {
          Errors = errors
        };
      }*/

      return new()
      {
        Body = await _repository.EditAsync(oldLeaveTime, _mapper.Map(request)),
        Errors = errors
      };
    }
  }
}
