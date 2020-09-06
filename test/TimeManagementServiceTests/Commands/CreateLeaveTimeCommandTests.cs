using FluentValidation;
using LT.DigitalOffice.TimeManagementService.Commands;
using LT.DigitalOffice.TimeManagementService.Commands.Interfaces;
using LT.DigitalOffice.TimeManagementService.Database.Entities;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models;
using LT.DigitalOffice.TimeManagementService.Repositories.Interfaces;
using LT.DigitalOffice.TimeManagementService.Enums;
using Moq;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementServiceUnitTests.Commands
{
    public class CreateLeaveTimeCommandTests
    {
        private Mock<IValidator<CreateLeaveTimeRequest>> validatorMock;
        private Mock<IMapper<CreateLeaveTimeRequest, DbLeaveTime>> mapperMock;
        private Mock<ILeaveTimeRepository> repositoryMock;
        private ICreateLeaveTimeCommand command;

        private CreateLeaveTimeRequest request;
        private DbLeaveTime createdLeaveTime;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            request = new CreateLeaveTimeRequest()
            {
                LeaveType = LeaveType.SickLeave,
                Comment = "I have a sore throat",
                StartTime = new DateTime(2020, 7, 24),
                EndTime = new DateTime(2020, 7, 27),
                WorkerUserId = Guid.NewGuid()
            };

            createdLeaveTime = new DbLeaveTime()
            {
                Id = Guid.NewGuid(),
                LeaveType = request.LeaveType,
                Comment = request.Comment,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                WorkerUserId = request.WorkerUserId
            };
        }

        [SetUp]
        public void SetUp()
        {
            validatorMock = new Mock<IValidator<CreateLeaveTimeRequest>>();
            mapperMock = new Mock<IMapper<CreateLeaveTimeRequest, DbLeaveTime>>();
            repositoryMock = new Mock<ILeaveTimeRepository>();

            command = new CreateLeaveTimeCommand(validatorMock.Object, mapperMock.Object, repositoryMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenValidatorThrowsException()
        {
            validatorMock
                .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                .Returns(false);

            Assert.Throws<ValidationException>(() => command.Execute(request));
            repositoryMock.Verify(repository => repository.CreateLeaveTime(It.IsAny<DbLeaveTime>()), Times.Never);
        }

        [Test]
        public void ShouldThrowExceptionWhenRepositoryThrowsException()
        {
            validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            mapperMock
                .Setup(x => x.Map(It.IsAny<CreateLeaveTimeRequest>()))
                .Returns(createdLeaveTime);

            repositoryMock
                .Setup(x => x.CreateLeaveTime(It.IsAny<DbLeaveTime>()))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => command.Execute(request));
        }

        [Test]
        public void ShouldCreateNewLeaveTime()
        {
            validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            mapperMock
                .Setup(x => x.Map(It.IsAny<CreateLeaveTimeRequest>()))
                .Returns(createdLeaveTime);

            repositoryMock
                .Setup(x => x.CreateLeaveTime(It.IsAny<DbLeaveTime>()))
                .Returns(createdLeaveTime.Id);

            Assert.AreEqual(createdLeaveTime.Id, command.Execute(request));
            repositoryMock.Verify(repository => repository.CreateLeaveTime(It.IsAny<DbLeaveTime>()), Times.Once);
        }
    }
}