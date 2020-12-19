using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Broker.Responses;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Exceptions;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces.Helpers;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text;

namespace LT.DigitalOffice.TimeManagementService.Validation.Helpers
{
    public class UserAssignmentValidator : IUserAssignmentValidator
    {
        private IRequestClient<IGetUserRequest> userRequestClient;
        private IAccessValidator acessValidator;

        public UserAssignmentValidator(
            [FromServices] IRequestClient<IGetUserRequest> userRequestClient,
            [FromServices] IAccessValidator acessValidator)
        {
            this.userRequestClient = userRequestClient;
            this.acessValidator = acessValidator;
        }

        public bool UserCanAssignUser(Guid currentUserId, Guid assignedUserId)
        {
            if (currentUserId == assignedUserId)
            {
                return true;
            }

            if (!acessValidator.IsAdmin()) // right not exist
            {
                throw new ForbiddenException("Not enough rights to add work time to add another user.");
            }
            else
            {
                try
                {
                    var userInfoResponse = userRequestClient.GetResponse<IOperationResult<IGetUserResponse>>(
                        IGetUserRequest.CreateObj(assignedUserId));

                    if (userInfoResponse.Result.Message.IsSuccess)
                    {
                        return userInfoResponse.Result.Message.Body.IsActive;
                    }

                    throw new NotFoundException($"User with id {assignedUserId} not exist.");
                }
                catch
                {
                    throw new Exception("It was not possible to check that the user exists.");
                }
            }
        }
    }
}
