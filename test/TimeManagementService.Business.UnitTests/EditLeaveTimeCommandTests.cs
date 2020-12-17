using FluentValidation;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Moq;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Business.UnitTests
{
    public class EditLeaveTimeCommandTests
    {
        private Mock<IValidator<EditLeaveTimeRequest>> validatorMock;
        private Mock<ILeaveTimeRepository> repositoryMock;
        private IEditLeaveTimeCommand command;

        private EditLeaveTimeRequest request;
        private Guid currentUserId;
        private DbLeaveTime editedLeaveTime;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            request = new EditLeaveTimeRequest
            {
                LeaveTimeId = Guid.NewGuid(),
                Patch = new JsonPatchDocument<DbLeaveTime>()
            };
            currentUserId = Guid.NewGuid();
            editedLeaveTime = new DbLeaveTime() {};
        }

        [SetUp]
        public void SetUp()
        {
            validatorMock = new Mock<IValidator<EditLeaveTimeRequest>>();
            repositoryMock = new Mock<ILeaveTimeRepository>();

            validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            repositoryMock
                .Setup(x => x.GetLeaveTimeById(It.IsAny<Guid>()))
                .Returns(editedLeaveTime);

            repositoryMock
                .Setup(x => x.EditLeaveTime(It.IsAny<DbLeaveTime>()))
                .Returns(true);

            command = new EditLeaveTimeCommand(validatorMock.Object, repositoryMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenValidatorThrowsException()
        {
            validatorMock
                .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                .Returns(false);

            Assert.Throws<ValidationException>(() => command.Execute(request, currentUserId));
            repositoryMock.Verify(repository => repository.EditLeaveTime(It.IsAny<DbLeaveTime>()), Times.Never);
        }

        [Test]
        public void ShouldThrowExceptionWhenRepositoryThrowsException()
        {
            repositoryMock
                .Setup(x => x.EditLeaveTime(It.IsAny<DbLeaveTime>()))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => command.Execute(request, currentUserId));
        }

        [Test]
        public void ShouldEditNewWorkTimeWhenDataIsValid()
        {
            Assert.AreEqual(true, command.Execute(request, currentUserId));
            repositoryMock.Verify(repository => repository.EditLeaveTime(It.IsAny<DbLeaveTime>()), Times.Once);
        }
    }
}