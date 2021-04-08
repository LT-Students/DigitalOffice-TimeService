using FluentValidation;
using FluentValidation.Results;
using LT.DigitalOffice.TimeService.Business.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Business.UnitTests
{
    public class CreateLeaveTimeCommandTests
    {
        private Mock<ICreateLeaveTimeRequestValidator> _validatorMock;
        private Mock<IDbLeaveTimeMapper> _mapperMock;
        private Mock<ILeaveTimeRepository> _repositoryMock;
        private ICreateLeaveTimeCommand _command;

        private CreateLeaveTimeRequest _request;
        private DbLeaveTime _createdLeaveTime;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _request = new CreateLeaveTimeRequest()
            {
                LeaveType = LeaveType.SickLeave,
                Comment = "I have a sore throat",
                StartTime = new DateTime(2020, 7, 24),
                EndTime = new DateTime(2020, 7, 27),
                WorkerUserId = Guid.NewGuid()
            };

            _createdLeaveTime = new DbLeaveTime()
            {
                Id = Guid.NewGuid(),
                LeaveType = (int)_request.LeaveType,
                Comment = _request.Comment,
                StartTime = _request.StartTime,
                EndTime = _request.EndTime,
                WorkerUserId = _request.WorkerUserId
            };
        }

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<ICreateLeaveTimeRequestValidator>();
            _mapperMock = new Mock<IDbLeaveTimeMapper>();
            _repositoryMock = new Mock<ILeaveTimeRepository>();

            _command = new CreateLeaveTimeCommand(_validatorMock.Object, _mapperMock.Object, _repositoryMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenValidatorThrowsException()
        {
            _validatorMock
                .Setup(x => x.Validate(It.IsAny<CreateLeaveTimeRequest>()))
                .Returns(new ValidationResult(
                    new List<ValidationFailure>
                    {
                        new ValidationFailure("test", "something", null)
                    }));

            Assert.Throws<ValidationException>(() => _command.Execute(_request));
            _repositoryMock.Verify(repository => repository.CreateLeaveTime(It.IsAny<DbLeaveTime>()), Times.Never);
        }

        [Test]
        public void ShouldThrowExceptionWhenRepositoryThrowsException()
        {
            _validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            _mapperMock
                .Setup(x => x.Map(It.IsAny<CreateLeaveTimeRequest>()))
                .Returns(_createdLeaveTime);

            _repositoryMock
                .Setup(x => x.CreateLeaveTime(It.IsAny<DbLeaveTime>()))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _command.Execute(_request));
        }

        [Test]
        public void ShouldCreateNewLeaveTimeWhenDataIsValid()
        {
            _validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            _mapperMock
                .Setup(x => x.Map(It.IsAny<CreateLeaveTimeRequest>()))
                .Returns(_createdLeaveTime);

            _repositoryMock
                .Setup(x => x.CreateLeaveTime(It.IsAny<DbLeaveTime>()))
                .Returns(_createdLeaveTime.Id);

            Assert.AreEqual(_createdLeaveTime.Id, _command.Execute(_request));
            _repositoryMock.Verify(repository => repository.CreateLeaveTime(It.IsAny<DbLeaveTime>()), Times.Once);
        }
    }
}