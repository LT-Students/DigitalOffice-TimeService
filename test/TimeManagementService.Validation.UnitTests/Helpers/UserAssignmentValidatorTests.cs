using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Broker.Responses;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Exceptions;
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
    public class GetUserResponse : IGetUserResponse
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
    }

    class UserAssignmentValidatorTests
    {
        private IUserAssignmentValidator validator;

        private Mock<IAccessValidator> accessValidatorMock;
        private Mock<IRequestClient<IGetUserRequest>> requestClientMock;
        private OperationResult<IGetUserResponse> operationResult;

        private readonly Guid assignedUserId = Guid.NewGuid();
        private readonly Guid currentUserId = Guid.NewGuid();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            accessValidatorMock = new Mock<IAccessValidator>();

            accessValidatorMock
                .Setup(x => x.IsAdmin())
                .Returns(true);

            BrokerSetUp();
            validator = new UserAssignmentValidator(requestClientMock.Object, accessValidatorMock.Object);
        }

        private void BrokerSetUp()
        {
            var responseClientMock = new Mock<Response<IOperationResult<IGetUserResponse>>>();
            requestClientMock = new Mock<IRequestClient<IGetUserRequest>>();

            operationResult = new OperationResult<IGetUserResponse>();

            requestClientMock.Setup(
                x => x.GetResponse<IOperationResult<IGetUserResponse>>(
                    IGetUserRequest.CreateObj(assignedUserId), default, default))
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
            operationResult.Body = new GetUserResponse { Id = Guid.NewGuid(), IsActive = false };

            Assert.False(validator.UserCanAssignUser(currentUserId, assignedUserId));
        }

        [Test]
        public void ShouldReturnsTrueWhenClientReturnsTrueIsActive()
        {
            operationResult.IsSuccess = true;
            operationResult.Errors = new List<string>();
            operationResult.Body = new GetUserResponse { Id = Guid.NewGuid(), IsActive = false };

            Assert.False(validator.UserCanAssignUser(currentUserId, assignedUserId));
        }

        [Test]
        public void ShouldThrowExceptionWhenClientReturnsFalseIsSuccess()
        {
            operationResult.IsSuccess = false;
            operationResult.Errors = new List<string>();
            operationResult.Body = null;

            Assert.Throws<Exception>(() => validator.UserCanAssignUser(currentUserId, assignedUserId));
        }

        [Test]
        public void ShouldThrowExceptionWhenUserIsNotAdminAndCanAssingOtherUser()
        {
            accessValidatorMock
                .Setup(x => x.IsAdmin())
                .Returns(false);

            Assert.Throws<ForbiddenException>(() => validator.UserCanAssignUser(currentUserId, assignedUserId));
        }

        [Test]
        public void ShouldReturnTrueWhenUserDesignatesHimself()
        {
            Assert.True(validator.UserCanAssignUser(currentUserId, currentUserId));
        }
    }
}
