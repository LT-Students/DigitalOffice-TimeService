using FluentValidation;
using FluentValidation.Results;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeManagementService.Business.UnitTests
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
                ProjectId = request.ProjectId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Title = request.Title,
                Description = request.Description,
                WorkerUserId = request.WorkerUserId
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
                .Setup(x => x.Validate(It.IsAny<CreateWorkTimeRequest>()))
                .Returns(new ValidationResult(
                    new List<ValidationFailure>
                    {
                        new ValidationFailure("test", "something", null)
                    }));

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
        public void ShouldCreateNewWorkTimeWhenDataIsValid()
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