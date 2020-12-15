using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Broker.Responses;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Exceptions;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeManagementService.Validation
{
    public class AssignProjectValidator : IAssignProjectValidator
    {
        private IRequestClient<IGetProjectUserRequest> projectUserRequestClient;

        public AssignProjectValidator(
            [FromServices] IRequestClient<IGetProjectUserRequest> projectUserRequestClient)
        {
            this.projectUserRequestClient = projectUserRequestClient;
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
    }
}
