using FluentValidation;
using LT.DigitalOffice.TimeManagementService.Commands;
using LT.DigitalOffice.TimeManagementService.Commands.Interfaces;
using LT.DigitalOffice.TimeManagementService.Database.Entities;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models;
using LT.DigitalOffice.TimeManagementService.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementServiceUnitTests.Commands
{
    public class CreateWorkTimeCommandTests
    {
        private Mock<IValidator<CreateWorkTimeRequest>> validatorMock;
        private Mock<IMapper<CreateWorkTimeRequest, DbWorkTime>> mapperMock;
        private Mock<IWorkTimeRepository> repositoryMock;
        private ICreateWorkTimeCommand command;

        private CreateWorkTimeRequest request;
        private DbWorkTime createdWorkTime;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            request = new CreateWorkTimeRequest()
            {
                ProjectId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 7, 29, 9, 0, 0),
                EndTime = new DateTime(2020, 7, 29, 17, 0, 0),
                Title = "I was working on a very important task",
                Description = "I was asleep. I love sleep. I hope I get paid for this.",
                WorkerUserId = Guid.NewGuid()
            };

            createdWorkTime = new DbWorkTime()
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 7, 29, 9, 0, 0),
                EndTime = new DateTime(2020, 7, 29, 9, 0, 0),
                Title = "I was working on a very important task",
                Description = "I was asleep. I love sleep. I hope I get paid for this.",
                WorkerUserId = Guid.NewGuid()
            };
        }

        [SetUp]
        public void SetUp()
        {
            validatorMock = new Mock<IValidator<CreateWorkTimeRequest>>();
            mapperMock = new Mock<IMapper<CreateWorkTimeRequest, DbWorkTime>>();
            repositoryMock = new Mock<IWorkTimeRepository>();

            command = new CreateWorkTimeCommand(validatorMock.Object, mapperMock.Object, repositoryMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenValidatorThrowsException()
        {
            validatorMock
                .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                .Returns(false);

            Assert.Throws<ValidationException>(() => command.Execute(request));
            repositoryMock.Verify(repository => repository.CreateWorkTime(It.IsAny<DbWorkTime>()), Times.Never);
        }

        [Test]
        public void ShouldThrowExceptionWhenRepositoryThrowsException()
        {
            validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            mapperMock
                .Setup(x => x.Map(It.IsAny<CreateWorkTimeRequest>()))
                .Returns(createdWorkTime);

            repositoryMock
                .Setup(x => x.CreateWorkTime(It.IsAny<DbWorkTime>()))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => command.Execute(request));
        }

        [Test]
        public void ShouldCreateNewWorkTime()
        {
            validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            mapperMock
                .Setup(x => x.Map(It.IsAny<CreateWorkTimeRequest>()))
                .Returns(createdWorkTime);

            repositoryMock
                .Setup(x => x.CreateWorkTime(It.IsAny<DbWorkTime>()))
                .Returns(createdWorkTime.Id);

            Assert.AreEqual(createdWorkTime.Id, command.Execute(request));
            repositoryMock.Verify(repository => repository.CreateWorkTime(It.IsAny<DbWorkTime>()), Times.Once);
        }
    }
}