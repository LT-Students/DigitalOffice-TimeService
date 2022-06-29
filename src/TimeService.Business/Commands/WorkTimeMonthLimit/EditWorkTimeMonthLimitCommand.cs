using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTimeMonthLimit.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Patch.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTimeMonthLimit.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTimeMonthLimit
{
  public class EditWorkTimeMonthLimitCommand : IEditWorkTimeMonthLimitCommand
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWorkTimeMonthLimitRepository _repository;
    private readonly IPatchDbWorkTimeMonthLimitMapper _mapper;
    private readonly IAccessValidator _accessValidator;
    private readonly IEditWorkTimeMonthLimitRequestValidator _validator;

    public EditWorkTimeMonthLimitCommand(
      IHttpContextAccessor httpContextAccessor,
      IWorkTimeMonthLimitRepository repository,
      IPatchDbWorkTimeMonthLimitMapper mapper,
      IAccessValidator accessValidator,
      IEditWorkTimeMonthLimitRequestValidator validator)
    {
      _httpContextAccessor = httpContextAccessor;
      _repository = repository;
      _mapper = mapper;
      _accessValidator = accessValidator;
      _validator = validator;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid workTimeMonthLimitId, JsonPatchDocument<EditWorkTimeMonthLimitRequest> request)
    {
      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime))
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;

        return new();
      }

      if (!_validator.ValidateCustom(request, out List<string> errors))
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return new()
        {
          Errors = errors
        };
      }

      bool result = await _repository.EditAsync(workTimeMonthLimitId, _mapper.Map(request));

      return new()
      {
        Body = result,
        Errors = new()
      };
    }
  }
}
