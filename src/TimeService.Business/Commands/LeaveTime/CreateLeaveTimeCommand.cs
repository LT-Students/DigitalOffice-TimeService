using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Helpers;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime
{
  public class CreateLeaveTimeCommand : ICreateLeaveTimeCommand
  {
    private readonly ICreateLeaveTimeRequestValidator _validator;
    private readonly IDbLeaveTimeMapper _mapper;
    private readonly ILeaveTimeRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IResponseCreator _responseCreator;
    private readonly ILeaveTimeAccessValidationHelper _ltAccessValidationHelper;

    public CreateLeaveTimeCommand(
      ICreateLeaveTimeRequestValidator validator,
      IDbLeaveTimeMapper mapper,
      ILeaveTimeRepository repository,
      IHttpContextAccessor httpContextAccessor,
      IResponseCreator responseCreator,
      ILeaveTimeAccessValidationHelper ltAccessValidationHelper)
    {
      _validator = validator;
      _mapper = mapper;
      _repository = repository;
      _httpContextAccessor = httpContextAccessor;
      _responseCreator = responseCreator;
      _ltAccessValidationHelper = ltAccessValidationHelper;
    }

    public async Task<OperationResultResponse<Guid?>> ExecuteAsync(CreateLeaveTimeRequest request)
    {
      bool isOwner = request.UserId == _httpContextAccessor.HttpContext.GetUserId();

      if (!isOwner && !await _ltAccessValidationHelper.HasRightsAsync(ltOwnerId: request.UserId))
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
