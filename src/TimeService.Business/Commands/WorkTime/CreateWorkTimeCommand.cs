using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime
{
  public class CreateWorkTimeCommand : ICreateWorkTimeCommand
  {
    private readonly IDbWorkTimeMapper _dbWorkTimeMapper;
    private readonly IWorkTimeRepository _workTimeRepository;
    private readonly ICreateWorkTimeRequestValidator _requestValidator;
    private readonly IResponseCreator _responseCreator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateWorkTimeCommand(
      IDbWorkTimeMapper dbWorkTimeMapper,
      IWorkTimeRepository workTimeRepository,
      ICreateWorkTimeRequestValidator requestValidator,
      IResponseCreator responseCreator,
      IHttpContextAccessor httpContextAccessor)
    {
      _dbWorkTimeMapper = dbWorkTimeMapper;
      _workTimeRepository = workTimeRepository;
      _requestValidator = requestValidator;
      _responseCreator = responseCreator;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OperationResultResponse<Guid?>> ExecuteAsync(CreateWorkTimeRequest request)
    {
      ValidationResult result = await _requestValidator.ValidateAsync(request);
      if (!result.IsValid)
      {
        return _responseCreator.CreateFailureResponse<Guid?>(HttpStatusCode.BadRequest,
          result.Errors.Select(x => x.ErrorMessage).ToList());
      }

      OperationResultResponse<Guid?> response = new();

      response.Body = await _workTimeRepository.CreateAsync(_dbWorkTimeMapper.Map(request));

      _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

      return response.Body is null
        ? _responseCreator.CreateFailureResponse<Guid?>(HttpStatusCode.BadRequest)
        : response;
    }
  }
}
