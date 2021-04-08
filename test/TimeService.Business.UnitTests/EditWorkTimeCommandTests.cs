using FluentValidation;
using FluentValidation.Results;
using LT.DigitalOffice.TimeService.Business.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Business.UnitTests
{
    public class EditWorkTimeCommandTests
    {
        private Mock<IEditWorkTimeRequestValidator> _validatorMock;
        private Mock<IDbWorkTimeMapper> _mapperMock;
        private Mock<IWorkTimeRepository> _repositoryMock;
        private IEditWorkTimeCommand _command;

        private EditWorkTimeRequest _request;
        private DbWorkTime _editedWorkTime;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _request = new EditWorkTimeRequest()
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 7, 29, 9, 0, 0),
                EndTime = new DateTime(2020, 7, 29, 17, 0, 0),
                Title = "I was working on a very important task",
                Description = "I was asleep. I love sleep. I hope I get paid for this.",
                WorkerUserId = Guid.NewGuid()
            };

            _editedWorkTime = new DbWorkTime()
            {
                Id = _request.Id,
                ProjectId = Guid.NewGuid(),
                StartTime = _request.StartTime,
                EndTime = _request.EndTime,
                Title = "I was working on a very very important task",
                Description = _request.Description,
                WorkerUserId = _request.WorkerUserId
            };
        }

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<IEditWorkTimeRequestValidator>();
            _mapperMock = new Mock<IDbWorkTimeMapper>();
            _repositoryMock = new Mock<IWorkTimeRepository>();

            _command = new EditWorkTimeCommand(_validatorMock.Object, _repositoryMock.Object, _mapperMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenValidatorThrowsException()
        {
            _validatorMock
                .Setup(x => x.Validate(It.IsAny<EditWorkTimeRequest>()))
                .Returns(new ValidationResult(
                    new List<ValidationFailure>
                    {
                        new ValidationFailure("test", "something", null)
                    }));

            Assert.Throws<ValidationException>(() => _command.Execute(_request));
            _repositoryMock.Verify(repository => repository.EditWorkTime(It.IsAny<DbWorkTime>()), Times.Never);
        }

        [Test]
        public void ShouldThrowExceptionWhenRepositoryThrowsException()
        {
            _validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            _mapperMock
                .Setup(x => x.Map(It.IsAny<EditWorkTimeRequest>()))
                .Returns(_editedWorkTime);

            _repositoryMock
                .Setup(x => x.EditWorkTime(It.IsAny<DbWorkTime>()))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _command.Execute(_request));
        }

        [Test]
        public void ShouldEditNewWorkTimeWhenDataIsValid()
        {
            _validatorMock
                 .Setup(x => x.Validate(It.IsAny<EditWorkTimeRequest>()).IsValid)
                 .Returns(true);

            _mapperMock
                .Setup(x => x.Map(It.IsAny<EditWorkTimeRequest>()))
                .Returns(_editedWorkTime);

            _repositoryMock
                .Setup(x => x.EditWorkTime(It.IsAny<DbWorkTime>()))
                .Returns(true);

            Assert.AreEqual(true, _command.Execute(_request));
            _repositoryMock.Verify(repository => repository.EditWorkTime(It.IsAny<DbWorkTime>()), Times.Once);
        }
    }
}