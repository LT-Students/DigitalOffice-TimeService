using System;
using System.Collections.Generic;
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
  public class CreateWorkTimeRequestValidator : AbstractValidator<CreateWorkTimeRequest>, ICreateWorkTimeRequestValidator
  {
    private readonly IRequestClient<ICheckUsersExistence> _rcCheckUsersExistence;
    private readonly ILogger<CreateLeaveTimeRequestValidator> _logger;
    private readonly IWorkTimeRepository _workTimeRepository;

    private async Task<bool> CheckUserExistenceAsync(List<Guid> userIds)
    {
      try
      {
        IOperationResult<ICheckUsersExistence> response = (await _rcCheckUsersExistence.GetResponse<IOperationResult<ICheckUsersExistence>>(
          ICheckUsersExistence.CreateObj(userIds))).Message;

        if (response.IsSuccess && response.Body.UserIds.Count == 1 && response.Body.UserIds[0] == userIds[0])
        {
          return true;
        }
        _logger.LogWarning($"Can not find users with these Ids '{userIds}': " +
          $"{Environment.NewLine}{string.Join('\n', response.Errors)}");
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, "Cannot check existing users { userIds}", userIds);
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

      RuleFor(request => request.UserId)
        .MustAsync(async (x, _) => await CheckUserExistenceAsync(new List<Guid> {x}))
        .WithMessage("User with this Id doesn't exist.")
        .MustAsync(async (x, _) => !await _workTimeRepository.DoesEmptyWorkTimeExistAsync(x, DateTime.UtcNow))
        .WithMessage("WorkTime for this month already exists.");
    }
  }
}
