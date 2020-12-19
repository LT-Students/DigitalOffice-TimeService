using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Broker.Responses;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Exceptions;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces.Helpers;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;

namespace LT.DigitalOffice.TimeManagementService.Validation.Helpers
{
    public class ProjectAssignmentValidator : IProjectAssignmentValidator
    {
        private IRequestClient<IGetProjectUserRequest> projectUserRequestClient;

        public ProjectAssignmentValidator(
            [FromServices] IRequestClient<IGetProjectUserRequest> projectUserRequestClient)
        {
            this.projectUserRequestClient = projectUserRequestClient;
        }

        public bool CanAssignUserToProject(Guid assignedUserId, Guid assignedProjectId)
        {
            try
            {
                var projectUserInfoResponse = projectUserRequestClient.GetResponse<IOperationResult<IGetProjectUserResponse>>(
                    IGetProjectUserRequest.CreateObj(assignedProjectId, assignedUserId));

                if (projectUserInfoResponse.Result.Message.IsSuccess)
                {
                    return projectUserInfoResponse.Result.Message.Body.IsActive;
                }

                throw new NotFoundException($"User with id {assignedUserId} cannot be assigned to a project with id {assignedProjectId}.");
            }
            catch
            {
                throw new Exception("It was not possible to check that the user exists.");
            }
        }
    }
}
