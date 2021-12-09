using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Models.Broker.Common;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Validation.WorkTime
{
  public class CreateWorkTimeRequestValidator : AbstractValidator<CreateWorkTimeRequest>, ICreateWorkTimeRequestValidator
  {
    private readonly IRequestClient<ICheckUsersExistence> _rcCheckUsersExistence;
    private readonly ILogger<CreateLeaveTimeRequestValidator> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWorkTimeRepository _workTimeRepository;

    private async Task<bool> CheckUserExistenceAsync(List<Guid> usersIds)
    {
      try
      {
        IOperationResult<ICheckUsersExistence> response = (await _rcCheckUsersExistence.GetResponse<IOperationResult<ICheckUsersExistence>>(
          ICheckUsersExistence.CreateObj(usersIds))).Message;

        if (response.IsSuccess && response.Body.UserIds.Any())
        {
          return true;
        }
        _logger.LogWarning($"Can not find users with these Ids: '{usersIds}': " +
          $"{Environment.NewLine}{string.Join('\n', response.Errors)}");
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, "Cannot check existing users with Ids: { userIds}", usersIds);
      }

      return false;
    }

    private bool IsMonthValid(int month)
    {
      DateTime dateTimeNow = DateTime.UtcNow;

      if(month == dateTimeNow.Month
        || (dateTimeNow.Day <= 5 && dateTimeNow.AddMonths(1).Month == month))
      {
        return true;
      }

      return false;
    }

    private bool IsYearValid(int year, int month)
    {
      DateTime dateTimeNow = DateTime.UtcNow;

      if (dateTimeNow.Year == year
        || dateTimeNow.AddMonths(-1).Year == year && month == 1)
      {
        return true;
      }

      return false;
    }

    public CreateWorkTimeRequestValidator(
      IRequestClient<ICheckUsersExistence> rcCheckUsersExistence,
      ILogger<CreateLeaveTimeRequestValidator> logger,
      IHttpContextAccessor httpContextAccessor,
      IWorkTimeRepository workTimeRepository)
    {
      _rcCheckUsersExistence = rcCheckUsersExistence;
      _logger = logger;
      _httpContextAccessor = httpContextAccessor;
      _workTimeRepository = workTimeRepository;

      RuleFor(_ => _httpContextAccessor.HttpContext.GetUserId())
        .MustAsync(async (x, _) => await CheckUserExistenceAsync(new List<Guid> { x }))
        .WithMessage("User with this Id doesn't exist.");

      RuleFor(request => request)
        .Must(request => IsMonthValid(request.Month)).WithMessage("Month is not valid.")
        .Must(request => IsYearValid(request.Year, request.Month)).WithMessage("Year is not valid.")
        .MustAsync(async (x, _) =>
          !await _workTimeRepository.DoesEmptyWorkTimeExistAsync(_httpContextAccessor.HttpContext.GetUserId(), x.Month, x.Year))
        .WithMessage("WorkTime for this month already exists.");
    }
  }
}
