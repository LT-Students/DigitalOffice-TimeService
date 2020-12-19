using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Broker.Responses;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Exceptions;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text;

namespace LT.DigitalOffice.TimeManagementService.Validation
{
    public class AssignUserValidator : IAssignUserValidator
    {
        private IRequestClient<IGetUserRequest> userRequestClient;
        private IAccessValidator acessValidator;

        public AssignUserValidator(
            [FromServices] IRequestClient<IGetUserRequest> userRequestClient,
            [FromServices] IAccessValidator acessValidator)
        {
            this.userRequestClient = userRequestClient;
            this.acessValidator = acessValidator;
        }

        public bool CanAssignUser(Guid currentUserId, Guid assignedUserId)
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

                    throw new Exception(new StringBuilder().AppendJoin("\n", userInfoResponse.Result.Message.Errors).ToString());
                }
                catch
                {
                    throw new Exception("It was not possible to check that the user exists.");
                }
            }
        }
    }
}
