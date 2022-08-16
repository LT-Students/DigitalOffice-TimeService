using System;
using System.Collections.Generic;
using FluentValidation;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Validation.WorkTime
{
  public class CreateWorkTimeRequestValidator : AbstractValidator<CreateWorkTimeRequest>, ICreateWorkTimeRequestValidator
  {
    private readonly IUserService _userService;
    private readonly ILogger<CreateLeaveTimeRequestValidator> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWorkTimeRepository _workTimeRepository;

    private bool IsMonthValid(int month, sbyte offset)
    {
      DateTime dateTimeNow = DateTime.UtcNow.AddHours(offset);

      if (month == dateTimeNow.Month
        || (dateTimeNow.Day <= 5 && dateTimeNow.AddMonths(1).Month == month))
      {
        return true;
      }

      return false;
    }

    private bool IsYearValid(int year, int month, sbyte offset)
    {
      DateTime dateTimeNow = DateTime.UtcNow.AddHours(offset);

      if (dateTimeNow.Year == year
        || dateTimeNow.AddMonths(-1).Year == year && month == 1)
      {
        return true;
      }

      return false;
    }

    public CreateWorkTimeRequestValidator(
      IUserService userService,
      ILogger<CreateLeaveTimeRequestValidator> logger,
      IHttpContextAccessor httpContextAccessor,
      IWorkTimeRepository workTimeRepository)
    {
      _userService = userService;
      _logger = logger;
      _httpContextAccessor = httpContextAccessor;
      _workTimeRepository = workTimeRepository;

      RuleFor(_ => _httpContextAccessor.HttpContext.GetUserId())
        .MustAsync(async (userId, _) => (await _userService.CheckUsersExistenceAsync(new List<Guid> { userId }))?.Count == 1)
        .WithMessage("User with this Id doesn't exist.");

      RuleFor(x => x.Description)
        .MaximumLength(500).WithMessage("Description is too long.");

      RuleFor(request => request)
        .Must(request => request.Offset >= -12 && request.Offset <= 12).WithMessage("Incorrect offset value.")
        .Must(request => IsMonthValid(request.Month, request.Offset)).WithMessage("Month is not valid.")
        .Must(request => IsYearValid(request.Year, request.Month, request.Offset)).WithMessage("Year is not valid.")
        .MustAsync(async (x, _) =>
          !await _workTimeRepository.DoesEmptyWorkTimeExistAsync(_httpContextAccessor.HttpContext.GetUserId(), x.Month, x.Year))
        .WithMessage("WorkTime for this month already exists.");
    }
  }
}
