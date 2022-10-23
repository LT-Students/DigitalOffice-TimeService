using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Helpers;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Business.Helpers.Workdays.Intergations.Interface;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime
{
  public class CreateLeaveTimeCommand : ICreateLeaveTimeCommand
  {
    private readonly ICreateLeaveTimeRequestValidator _validator;
    private readonly IDbLeaveTimeMapper _mapper;
    private readonly ILeaveTimeRepository _leaveTimeRepository;
    private readonly IWorkTimeMonthLimitRepository _workTimeMonthLimitRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IResponseCreator _responseCreator;
    private readonly ILeaveTimeAccessValidationHelper _ltAccessValidationHelper;
    private readonly ICompanyService _companyService;
    private readonly ICalendar _calendar;
    private readonly ILogger<CreateLeaveTimeCommand> _logger;

    private async Task<string> GetHolidaysAsync(int month, int year)
    {
      string holidays = (await _workTimeMonthLimitRepository.GetAsync(month: month, year: year))?.Holidays;

      if (holidays is null)
      {
        _logger.LogError($"Can't find WorkTimeMonthLimit with month: {month}, year: {year} in database.");

        try
        {
          holidays = await _calendar.GetWorkCalendarByMonthAsync(month: month, year: year);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, $"An exception occurred while trying get holidays for month: {month}, year: {year}.");
        }
      }

      return holidays;
    }

    public CreateLeaveTimeCommand(
      ICreateLeaveTimeRequestValidator validator,
      IDbLeaveTimeMapper mapper,
      ILeaveTimeRepository leaveTimeRepository,
      IWorkTimeMonthLimitRepository workTimeMonthLimitRepository,
      IHttpContextAccessor httpContextAccessor,
      IResponseCreator responseCreator,
      ILeaveTimeAccessValidationHelper ltAccessValidationHelper,
      ICompanyService companyService,
      ICalendar calendar,
      ILogger<CreateLeaveTimeCommand> logger)
    {
      _validator = validator;
      _mapper = mapper;
      _leaveTimeRepository = leaveTimeRepository;
      _workTimeMonthLimitRepository = workTimeMonthLimitRepository;
      _httpContextAccessor = httpContextAccessor;
      _responseCreator = responseCreator;
      _ltAccessValidationHelper = ltAccessValidationHelper;
      _companyService = companyService;
      _calendar = calendar;
      _logger = logger;
    }

    public async Task<OperationResultResponse<Guid?>> ExecuteAsync(CreateLeaveTimeRequest request)
    {
      bool isOwner = request.UserId == _httpContextAccessor.HttpContext.GetUserId();

      // will uncomment it after implementing manager's leave times
      if (!isOwner /*&& !await _ltAccessValidationHelper.HasRightsAsync(ltOwnerId: request.UserId)*/)
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

      double? rate = null;
      string holidays = null;

      if (request.LeaveType == Models.Dto.Enums.LeaveType.Prolonged)
      {
        rate = (await _companyService.GetCompaniesDataAsync(usersIds: new() { request.UserId }, errors: default))
          ?.SelectMany(cd => cd.Users)?.FirstOrDefault(u => u.UserId == request.UserId)?.Rate;

        holidays = await GetHolidaysAsync(request.StartTime.Month, request.StartTime.Year);

        if (holidays is null)
        {
          return _responseCreator.CreateFailureResponse<Guid?>(HttpStatusCode.BadGateway);
        }
      }

      response.Body = await _leaveTimeRepository.CreateAsync(_mapper.Map(request, rate, holidays));

      _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

      return response.Body is null
        ? _responseCreator.CreateFailureResponse<Guid?>(HttpStatusCode.BadRequest)
        : response;
    }
  }
}
