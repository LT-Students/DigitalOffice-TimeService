using FluentValidation;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Models.Broker.Common;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Validation.LeaveTime
{
    public class CreateLeaveTimeRequestValidator : AbstractValidator<CreateLeaveTimeRequest>, ICreateLeaveTimeRequestValidator
    {
        private readonly IRequestClient<ICheckUserExistence> _rcCheckUsersExistence;
        private readonly ILogger<CreateLeaveTimeRequestValidator> _logger;

        private bool CheckUserExistence(List<Guid> userIds)
        {
            string logMessage = "Cannot check existing users {userIds}";

            try
            {
                var s = ICheckUserExistence.CreateObj(userIds);
                IOperationResult<ICheckUserExistence> response = _rcCheckUsersExistence.GetResponse<IOperationResult<ICheckUserExistence>>(
                    s).Result.Message;
                if (response.IsSuccess && response.Body.UserIds.Count == 1)
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
            IRequestClient<ICheckUserExistence> rcCheckUsersExistence,
            ILogger<CreateLeaveTimeRequestValidator> logger)
        {
            _rcCheckUsersExistence = rcCheckUsersExistence;
            _logger = logger;

            RuleFor(lt => lt.UserId)
                .NotEmpty()
                .Must(UserId => CheckUserExistence(new List<Guid>() { UserId }))
                .WithMessage("Project users don't exist.");
            /*.Must(lt =>
            {
                List<Guid> Ids = new();
                Ids.Add(lt.UserId);
                var existUsers = CheckUserExistence(Ids);
                return existUsers.UserIds.Count == 0;
            }).WithMessage("The user must participate in the project.");*/

            /*RuleFor(lt => lt)
                .Must(lt =>
                {
                    *//*List<Guid> Ids = new();
                    Ids.Add(lt.UserId);
                    var existUsers = CheckUserExistence(Ids);
                    return existUsers.UserIds.Count != 0;*//*

                }).WithMessage("Project users don't exist.");*/

            RuleFor(lt => lt.LeaveType)
                .IsInEnum();

            RuleFor(lt => lt.StartTime)
                .NotEqual(new DateTime());

            RuleFor(lt => lt.EndTime)
                .NotEqual(new DateTime());

            RuleFor(lt => lt)
                .Must(lt => lt.StartTime < lt.EndTime).WithMessage("Start time must be before end time.")
                .Must(lt =>
                {
                    var leaveTimes = repository.Find(new FindLeaveTimesFilter { UserId = lt.UserId }, 0, int.MaxValue, out _);

                    return leaveTimes.All(oldLeaveTime =>
                        lt.EndTime <= oldLeaveTime.StartTime || oldLeaveTime.EndTime <= lt.StartTime);
                }).WithMessage("New LeaveTime should not overlap with old ones.");
        }
    }
}
