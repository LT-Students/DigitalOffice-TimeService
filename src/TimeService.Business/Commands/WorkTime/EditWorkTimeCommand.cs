using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
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
    private readonly IAccessValidator _accessValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IResponseCreator _responseCreator;

    public EditWorkTimeCommand(
      IEditWorkTimeRequestValidator validator,
      IWorkTimeRepository repository,
      IPatchDbWorkTimeMapper patchMapper,
      IDbWorkTimeMapper dbMapper,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor,
      IResponseCreator responseCreator)
    {
      _validator = validator;
      _repository = repository;
      _patchMapper = patchMapper;
      _dbMapper = dbMapper;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
      _responseCreator = responseCreator;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid workTimeId, JsonPatchDocument<EditWorkTimeRequest> request)
    {
      DbWorkTime oldDbWorkTime = await _repository.GetAsync(workTimeId);

      bool isOwner = _httpContextAccessor.HttpContext.GetUserId() == oldDbWorkTime.UserId;
      if (!isOwner && !await _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      if (!_validator.ValidateCustom(request, out List<string> errors))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.BadRequest, errors);
      }

      if (!isOwner)
      {
        if (oldDbWorkTime.ManagerWorkTime == null)
        {
          DbWorkTime managerWorkTime = _dbMapper.Map(oldDbWorkTime, _httpContextAccessor.HttpContext.GetUserId());

          _patchMapper.Map(request).ApplyTo(managerWorkTime);

          bool result = (await _repository.CreateAsync(managerWorkTime)).HasValue;

          return new OperationResultResponse<bool>
          {
            Body = result,
            Errors = new()
          };
        }
        else
        {
          return new OperationResultResponse<bool>
          {
            Body = await _repository.EditAsync(oldDbWorkTime.ManagerWorkTime, _patchMapper.Map(request)),
            Errors = new()
          };
        }
      }

      return new OperationResultResponse<bool>
      {
        Body = await _repository.EditAsync(oldDbWorkTime, _patchMapper.Map(request)),
        Errors = new()
      };
    }
  }
}
