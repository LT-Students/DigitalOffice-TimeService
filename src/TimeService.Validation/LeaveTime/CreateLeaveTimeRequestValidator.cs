using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Models.Broker.Common;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TimeService.Validation.LeaveTime
{
  public class CreateLeaveTimeRequestValidator : AbstractValidator<CreateLeaveTimeRequest>, ICreateLeaveTimeRequestValidator
  {
    private readonly IRequestClient<ICheckUsersExistence> _rcCheckUsersExistence;
    private readonly ILogger<CreateLeaveTimeRequestValidator> _logger;

    private async Task<bool> CheckUserExistenceAsync(List<Guid> userIds)
    {
      string logMessage = "Cannot check existing users {userIds}";

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
        _logger.LogError(exc, logMessage);
      }

      return false;
    }

    private bool CheckLeaveTimeInterval(CreateLeaveTimeRequest lt)
    {
      DateTime timeNow = DateTime.UtcNow;

      switch (lt.LeaveType)
      {
        case LeaveType.SickLeave:
          if (lt.StartTime < timeNow.AddMonths(-1) || lt.EndTime > timeNow.AddMonths(1))
          {
            return false;
          }
          break;

        default:
          if (lt.StartTime < timeNow.AddMonths(-1) || (lt.StartTime.Month == timeNow.AddMonths(-1).Month && timeNow.Day > 5))
          {
            return false;
          }
          break;
      }

      return true;
    }

    public CreateLeaveTimeRequestValidator(
      ILeaveTimeRepository repository,
      IRequestClient<ICheckUsersExistence> rcCheckUsersExistence,
      ILogger<CreateLeaveTimeRequestValidator> logger)
    {
      _rcCheckUsersExistence = rcCheckUsersExistence;
      _logger = logger;

      RuleFor(lt => lt.UserId)
        .NotEmpty()
        .MustAsync(async (userId, cancellation) => await CheckUserExistenceAsync(new List<Guid>() { userId }))
        .WithMessage("This user doesn't exist.");

      RuleFor(lt => lt.LeaveType)
        .IsInEnum();

      RuleFor(lt => lt.StartTime)
        .NotEqual(new DateTime());

      RuleFor(lt => lt.EndTime)
        .NotEqual(new DateTime());

      RuleFor(lt => lt.Minutes)
        .GreaterThan(0);

      RuleFor(lt => lt)
        .Cascade(CascadeMode.Stop)
        .Must(lt => lt.StartTime <= lt.EndTime)
        .WithMessage("Start time must be before end time.")
        .Must(lt => CheckLeaveTimeInterval(lt))
        .WithMessage("Incorrect interval for leave time.")
        .MustAsync(async (lt, _) => !await repository.HasOverlapAsync(lt.UserId, lt.StartTime, lt.EndTime))
        .WithMessage("New LeaveTime should not overlap with old ones.");

      RuleFor(lt => lt.Comment)
        .MaximumLength(500).WithMessage("Comment is too long.");
    }
  }
}
