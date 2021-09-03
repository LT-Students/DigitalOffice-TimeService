﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Models.Broker.Common;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
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

    private bool CheckUserExistence(List<Guid> userIds)
    {
      string logMessage = "Cannot check existing users {userIds}";

      try
      {
        IOperationResult<ICheckUsersExistence> response = _rcCheckUsersExistence.GetResponse<IOperationResult<ICheckUsersExistence>>(
            ICheckUsersExistence.CreateObj(userIds)).Result.Message;
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

    public CreateLeaveTimeRequestValidator(
        ILeaveTimeRepository repository,
        IRequestClient<ICheckUsersExistence> rcCheckUsersExistence,
        ILogger<CreateLeaveTimeRequestValidator> logger)
    {
      _rcCheckUsersExistence = rcCheckUsersExistence;
      _logger = logger;

      RuleFor(lt => lt.UserId)
        .NotEmpty()
        .Must(UserId => CheckUserExistence(new List<Guid>() { UserId }))
        .WithMessage("This user doesn't exist.");

      RuleFor(lt => lt.LeaveType)
        .IsInEnum();

      RuleFor(lt => lt.StartTime)
        .NotEqual(new DateTime());

      RuleFor(lt => lt.EndTime)
        .NotEqual(new DateTime());

      //TODO rework line 70
      RuleFor(lt => lt)
        .Must(lt => lt.StartTime < lt.EndTime).WithMessage("Start time must be before end time.")
        .Must(lt =>
        {
          var leaveTimes = repository.Find(new FindLeaveTimesFilter { UserId = lt.UserId, SkipCount = 0, TakeCount = int.MaxValue }, out _);

          return leaveTimes.All(oldLeaveTime =>
                    lt.EndTime <= oldLeaveTime.StartTime || oldLeaveTime.EndTime <= lt.StartTime);
        }).WithMessage("New LeaveTime should not overlap with old ones.");
    }
  }
}
