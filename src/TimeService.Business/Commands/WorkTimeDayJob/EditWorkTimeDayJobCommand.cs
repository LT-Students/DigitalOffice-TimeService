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
using LT.DigitalOffice.TimeService.Business.Commands.WorkTimeDayJob.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Patch.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTimeDayJob.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTimeDayJob
{
  public class EditWorkTimeDayJobCommand : IEditWorkTimeDayJobCommand
  {
    private readonly IEditWorkTimeDayJobRequestValidator _validator;
    private readonly IAccessValidator _accessValidator;
    private readonly IPatchDbWorkTimeDayJobMapper _mapper;
    private readonly IWorkTimeDayJobRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EditWorkTimeDayJobCommand(
      IEditWorkTimeDayJobRequestValidator validator,
      IAccessValidator accessValidator,
      IPatchDbWorkTimeDayJobMapper mapper,
      IWorkTimeDayJobRepository repository,
      IHttpContextAccessor httpContextAccessor)
    {
      _validator = validator;
      _accessValidator = accessValidator;
      _mapper = mapper;
      _repository = repository;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid workTimeDayJobId, JsonPatchDocument<EditWorkTimeDayJobRequest> request)
    {
      DbWorkTimeDayJob dayJob = _repository.Get(workTimeDayJobId, true);
      Guid authorId = _httpContextAccessor.HttpContext.GetUserId();

      if (dayJob.WorkTime.UserId != authorId
        && !await _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime))
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
        Status = OperationResultStatusType.FullSuccess,
        Body = _repository.Edit(workTimeDayJobId, _mapper.Map(request)),
        Errors = new()
      };
    }
  }
}
