using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Broker.Responses;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.TimeManagementService.Validation.Helpers;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces.Helpers;
using LT.DigitalOffice.TimeManagementService.Validation.UnitTests.Utils;
using MassTransit;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LT.DigitalOffice.TimeManagementService.Validation.UnitTests.Helpers
{
    public class GetProjectUserResponse : IGetProjectUserResponse
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
    }

    class ProjectAssignmentValidatorTests
    {
        private IProjectAssignmentValidator validator;

        private Mock<IRequestClient<IGetProjectUserRequest>> requestClientMock;
        private OperationResult<IGetProjectUserResponse> operationResult;

        private readonly Guid assignedUserId = Guid.NewGuid();
        private readonly Guid projectId = Guid.NewGuid();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            BrokerSetUp();
            validator = new ProjectAssignmentValidator(requestClientMock.Object);
        }

        private void BrokerSetUp()
        {
            var responseClientMock = new Mock<Response<IOperationResult<IGetProjectUserResponse>>>();
            requestClientMock = new Mock<IRequestClient<IGetProjectUserRequest>>();

            operationResult = new OperationResult<IGetProjectUserResponse>();

            requestClientMock.Setup(
                x => x.GetResponse<IOperationResult<IGetProjectUserResponse>>(
                    IGetProjectUserRequest.CreateObj(projectId, assignedUserId), default, default))
                .Returns(Task.FromResult(responseClientMock.Object));

            responseClientMock
                .SetupGet(x => x.Message)
                .Returns(operationResult);
        }

        [Test]
        public void ShouldReturnsFalseWhenClientReturnsFalseIsActive()
        {
            operationResult.IsSuccess = true;
            operationResult.Errors = new List<string>();
            operationResult.Body = new GetProjectUserResponse { Id = Guid.NewGuid(), IsActive = false };

            Assert.False(validator.CanAssignUserToProject(assignedUserId, projectId));
        }

        [Test]
        public void ShouldReturnsTrueWhenClientReturnsTrueIsActive()
        {
            operationResult.IsSuccess = true;
            operationResult.Errors = new List<string>();
            operationResult.Body = new GetProjectUserResponse { Id = Guid.NewGuid(), IsActive = false };

            Assert.False(validator.CanAssignUserToProject(assignedUserId, projectId));
        }

        [Test]
        public void ShouldThrowExceptionWhenClientReturnsFalseIsSuccess()
        {
            operationResult.IsSuccess = false;
            operationResult.Errors = new List<string>();
            operationResult.Body = null;

            Assert.Throws<Exception>(() => validator.CanAssignUserToProject(assignedUserId, projectId));
        }
    }
}
