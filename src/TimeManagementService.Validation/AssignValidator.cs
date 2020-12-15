using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Broker.Responses;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Exceptions;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace LT.DigitalOffice.TimeManagementService.Validation
{
    public class AssignValidator : IAssignValidator
    {
        private IRequestClient<IGetUserRequest> userRequestClient;
        private IRequestClient<IGetProjectUserRequest> projectUserRequestClient;
        private IAccessValidator acessValidator;

        public AssignValidator(
            [FromServices] IRequestClient<IGetUserRequest> userRequestClient,
            [FromServices] IRequestClient<IGetProjectUserRequest> projectUserRequestClient,
            [FromServices] IAccessValidator acessValidator)
        {
            this.userRequestClient = userRequestClient;
            this.projectUserRequestClient = projectUserRequestClient;
            this.acessValidator = acessValidator;
        }

        public bool CanAssignProject(Guid assignedUserId, Guid assignedProjectId)
        {
            try
            {
                var projectUserInfoResponse = projectUserRequestClient.GetResponse<IOperationResult<IGetProjectUserResponse>>(
                    IGetProjectUserRequest.CreateObj(assignedUserId, assignedProjectId));

                if (projectUserInfoResponse.Result.Message.IsSuccess)
                {
                    return projectUserInfoResponse.Result.Message.Body.IsActive;
                }

                throw new Exception("It was not possible to check that the user exists.");
            }
            catch
            {
                throw new Exception("It was not possible to check that the user exists.");
            }
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
                        return true;
                    }

                    throw new Exception("It was not possible to check that the user exists.");
                }
                catch
                {
                    throw new Exception("It was not possible to check that the user exists.");
                }
            }
        }
    }
}
