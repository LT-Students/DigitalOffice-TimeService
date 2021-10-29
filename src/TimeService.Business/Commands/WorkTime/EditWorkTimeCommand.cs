using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
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
    private readonly IPatchDbWorkTimeMapper _mapper;
    private readonly IAccessValidator _accessValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EditWorkTimeCommand(
      IEditWorkTimeRequestValidator validator,
      IWorkTimeRepository repository,
      IPatchDbWorkTimeMapper mapper,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor)
    {
      _validator = validator;
      _repository = repository;
      _mapper = mapper;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid workTimeId, JsonPatchDocument<EditWorkTimeRequest> request)
    {
      DbWorkTime oldDbWorkTime = await _repository.GetAsync(workTimeId);

      bool isOwner = _httpContextAccessor.HttpContext.GetUserId() == oldDbWorkTime.UserId;
      if (!isOwner && !await _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime))
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

        return new()
        {
          Status = OperationResultStatusType.Failed
        };
      }

      if (!_validator.ValidateCustom(request, out List<string> errors))
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return new()
        {
          Status = OperationResultStatusType.Failed,
          Errors = errors
        };
      }

      return new OperationResultResponse<bool>
      {
        Body = await _repository.EditAsync(oldDbWorkTime, _mapper.Map(request)),
        Status = OperationResultStatusType.FullSuccess,
        Errors = new()
      };
    }
  }
}
