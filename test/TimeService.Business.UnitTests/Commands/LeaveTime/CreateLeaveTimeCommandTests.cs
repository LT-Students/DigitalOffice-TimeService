using FluentValidation;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Business.UnitTests.Commands.LeaveTime
{
    public class CreateLeaveTimeCommandTests
    {
        private Mock<ICreateLeaveTimeRequestValidator> _validatorMock;
        private Mock<IDbLeaveTimeMapper> _mapperMock;
        private Mock<ILeaveTimeRepository> _repositoryMock;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private ICreateLeaveTimeCommand _command;

        private CreateLeaveTimeRequest _request;
        private DbLeaveTime _createdLeaveTime;
        private Guid _createdBy;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _createdBy = Guid.NewGuid();

            var items = new Dictionary<object, object>
            {
                { "UserId", _createdBy }
            };

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContextAccessorMock
                .Setup(x => x.HttpContext.Items)
                .Returns(items);

            _request = new CreateLeaveTimeRequest()
            {
                LeaveType = LeaveType.SickLeave,
                Comment = "I have a sore throat",
                StartTime = new DateTime(2020, 7, 24),
                EndTime = new DateTime(2020, 7, 27),
                UserId = Guid.NewGuid()
            };

            _createdLeaveTime = new DbLeaveTime()
            {
                Id = Guid.NewGuid(),
                CreatedBy = _createdBy,
                LeaveType = (int)_request.LeaveType,
                Comment = _request.Comment,
                StartTime = _request.StartTime,
                EndTime = _request.EndTime,
                UserId = _request.UserId
            };
        }

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<ICreateLeaveTimeRequestValidator>();
            _mapperMock = new Mock<IDbLeaveTimeMapper>();
            _repositoryMock = new Mock<ILeaveTimeRepository>();

            _command = new CreateLeaveTimeCommand(_validatorMock.Object, _mapperMock.Object, _repositoryMock.Object, _httpContextAccessorMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenValidatorThrowsException()
        {
            _validatorMock
                .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                .Returns(false);

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
                .Setup(x => x.Map(It.IsAny<CreateLeaveTimeRequest>(), _createdBy))
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
                .Setup(x => x.Map(It.IsAny<CreateLeaveTimeRequest>(), _createdBy))
                .Returns(_createdLeaveTime);

            _repositoryMock
                .Setup(x => x.CreateLeaveTime(It.IsAny<DbLeaveTime>()))
                .Returns(_createdLeaveTime.Id);

            Assert.AreEqual(_createdLeaveTime.Id, _command.Execute(_request));
            _repositoryMock.Verify(repository => repository.CreateLeaveTime(It.IsAny<DbLeaveTime>()), Times.Once);
        }
    }
}