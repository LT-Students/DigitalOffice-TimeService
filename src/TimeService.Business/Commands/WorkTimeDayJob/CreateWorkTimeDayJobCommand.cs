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
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTimeDayJob.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTimeDayJob
{
  public class CreateWorkTimeDayJobCommand : ICreateWorkTimeDayJobCommand
  {
    private readonly ICreateWorkTimeDayJobRequestValidator _validator;
    private readonly IAccessValidator _accessValidator;
    private readonly IDbWorkTimeDayJobMapper _mapper;
    private readonly IWorkTimeDayJobRepository _repository;
    private readonly IWorkTimeRepository _workTimeRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateWorkTimeDayJobCommand(
      ICreateWorkTimeDayJobRequestValidator validator,
      IAccessValidator accessValidator,
      IDbWorkTimeDayJobMapper mapper,
      IWorkTimeDayJobRepository repository,
      IWorkTimeRepository workTimeRepository,
      IHttpContextAccessor httpContextAccessor)
    {
      _validator = validator;
      _accessValidator = accessValidator;
      _mapper = mapper;
      _repository = repository;
      _workTimeRepository = workTimeRepository;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OperationResultResponse<Guid?>> ExecuteAsync(CreateWorkTimeDayJobRequest request)
    {
      Guid authorId = _httpContextAccessor.HttpContext.GetUserId();

      DbWorkTime workTime = await _workTimeRepository.GetAsync(request.WorkTimeId);

      if (authorId != workTime.UserId
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

      OperationResultResponse<Guid?> response = new();

      response.Body = await _repository.CreateAsync(_mapper.Map(request));
      response.Status = OperationResultStatusType.FullSuccess;

      _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

      if (response.Body == default)
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        response.Status = OperationResultStatusType.Failed;
      }

      return response;
    }
  }
}
