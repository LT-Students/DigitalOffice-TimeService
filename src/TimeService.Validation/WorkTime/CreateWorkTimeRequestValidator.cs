using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Models.Broker.Common;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Validation.WorkTime
{
  public class CreateWorkTimeRequestValidator : AbstractValidator<(CreateWorkTimeRequest request, Guid userId)>, ICreateWorkTimeRequestValidator
  {
    private readonly IRequestClient<ICheckUsersExistence> _rcCheckUsersExistence;
    private readonly ILogger<CreateLeaveTimeRequestValidator> _logger;
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
      IWorkTimeRepository workTimeRepository)
    {
      _rcCheckUsersExistence = rcCheckUsersExistence;
      _logger = logger;
      _workTimeRepository = workTimeRepository;

      RuleFor(tuple => tuple.userId)
        .MustAsync(async (x, _) => await CheckUserExistenceAsync(new List<Guid> {x}))
        .WithMessage("User with this Id doesn't exist.")
        .MustAsync(async (x, _) => !await _workTimeRepository.DoesEmptyWorkTimeExistAsync(x, DateTime.UtcNow))
        .WithMessage("WorkTime for this month already exists.");

      When(
        tuple => tuple.request.Month is not null,
        () =>
          RuleFor(tuple => tuple.request.Month)
          .Must(month => IsMonthValid(month.Value)).WithMessage("Month is not valid.")
        );

      When(
        tuple => tuple.request.Year is not null,
        () =>
          RuleFor(tuple => tuple.request)
          .Must(request => IsYearValid(request.Year.Value, request.Month.Value)).WithMessage("Year is not valid.")
        );
    }
  }
}
