using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
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
    private readonly IResponseCreator _responseCreator;
    private readonly IDbWorkTimeDayJobMapper _mapper;
    private readonly IWorkTimeDayJobRepository _repository;
    private readonly IWorkTimeRepository _workTimeRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateWorkTimeDayJobCommand(
      ICreateWorkTimeDayJobRequestValidator validator,
      IAccessValidator accessValidator,
      IResponseCreator responseCreator,
      IDbWorkTimeDayJobMapper mapper,
      IWorkTimeDayJobRepository repository,
      IWorkTimeRepository workTimeRepository,
      IHttpContextAccessor httpContextAccessor)
    {
      _validator = validator;
      _accessValidator = accessValidator;
      _responseCreator = responseCreator;
      _mapper = mapper;
      _repository = repository;
      _workTimeRepository = workTimeRepository;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OperationResultResponse<Guid?>> ExecuteAsync(CreateWorkTimeDayJobRequest request)
    {
      Guid authorId = _httpContextAccessor.HttpContext.GetUserId();

      DbWorkTime workTime = await _workTimeRepository.GetAsync(request.WorkTimeId);

      if (authorId != workTime?.UserId
        && !await _accessValidator.HasRightsAsync(Rights.AddEditRemoveTime))
      {
        return _responseCreator.CreateFailureResponse<Guid?>(HttpStatusCode.Forbidden);
      }

      ValidationResult validationResult = await _validator.ValidateAsync(request);
      if (!validationResult.IsValid)
      {
        return _responseCreator.CreateFailureResponse<Guid?>(
          HttpStatusCode.BadRequest,
          validationResult.Errors.Select(e => e.ErrorMessage).ToList());
      }

      OperationResultResponse<Guid?> response = new();

      response.Body = await _repository.CreateAsync(_mapper.Map(request));

      _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

      return response.Body is null
        ? _responseCreator.CreateFailureResponse<Guid?>(HttpStatusCode.BadRequest)
        : response;
    }
  }
}
