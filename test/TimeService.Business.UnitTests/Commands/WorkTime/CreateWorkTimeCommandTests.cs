using FluentValidation;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using Moq;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeService.Business.UnitTests.Commands.WorkTime
{
    public class CreateWorkTimeCommandTests
    {
        private Mock<ICreateWorkTimeRequestValidator> _validatorMock;
        private Mock<IDbWorkTimeMapper> _mapperMock;
        private Mock<IWorkTimeRepository> _repositoryMock;
        private ICreateWorkTimeCommand _command;

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
                UserId = request.WorkerUserId
            };
        }

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<ICreateWorkTimeRequestValidator>();
            _mapperMock = new Mock<IDbWorkTimeMapper>();
            _repositoryMock = new Mock<IWorkTimeRepository>();

            _command = new CreateWorkTimeCommand(_validatorMock.Object, _mapperMock.Object, _repositoryMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenValidatorThrowsException()
        {
            _validatorMock
                .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                .Returns(false);

            Assert.Throws<ValidationException>(() => _command.Execute(request));
            _repositoryMock.Verify(repository => repository.CreateWorkTime(It.IsAny<DbWorkTime>()), Times.Never);
        }

        [Test]
        public void ShouldThrowExceptionWhenRepositoryThrowsException()
        {
            _validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            _mapperMock
                .Setup(x => x.Map(It.IsAny<CreateWorkTimeRequest>()))
                .Returns(createdWorkTime);

            _repositoryMock
                .Setup(x => x.CreateWorkTime(It.IsAny<DbWorkTime>()))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _command.Execute(request));
        }

        [Test]
        public void ShouldCreateNewWorkTimeWhenDataIsValid()
        {
            _validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            _mapperMock
                .Setup(x => x.Map(It.IsAny<CreateWorkTimeRequest>()))
                .Returns(createdWorkTime);

            _repositoryMock
                .Setup(x => x.CreateWorkTime(It.IsAny<DbWorkTime>()))
                .Returns(createdWorkTime.Id);

            Assert.AreEqual(createdWorkTime.Id, _command.Execute(request));
            _repositoryMock.Verify(repository => repository.CreateWorkTime(It.IsAny<DbWorkTime>()), Times.Once);
        }
    }
}