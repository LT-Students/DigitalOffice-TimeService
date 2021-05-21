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
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.UnitTestKernel;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Exceptions.Models;

namespace LT.DigitalOffice.TimeService.Business.UnitTests.Commands.LeaveTime
{
    public class CreateLeaveTimeCommandTests
    {
        private Mock<ICreateLeaveTimeRequestValidator> _validatorMock;
        private Mock<IDbLeaveTimeMapper> _mapperMock;
        private Mock<ILeaveTimeRepository> _repositoryMock;
        private Mock<IAccessValidator> _accessValidatorMock;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private ICreateLeaveTimeCommand _command;

        private CreateLeaveTimeRequest _request;
        private DbLeaveTime _createdLeaveTime;
        private Guid _createdBy;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _createdBy = Guid.NewGuid();

            _request = new CreateLeaveTimeRequest()
            {
                LeaveType = LeaveType.SickLeave,
                Comment = "I have a sore throat",
                StartTime = new DateTime(2020, 7, 24),
                EndTime = new DateTime(2020, 7, 27),
                UserId = _createdBy
            };

            _createdLeaveTime = new DbLeaveTime()
            {
                Id = Guid.NewGuid(),
                CreatedBy = _createdBy,
                LeaveType = (int)_request.LeaveType,
                Comment = _request.Comment,
                StartTime = _request.StartTime,
                EndTime = _request.EndTime,
                UserId = _createdBy
            };
        }

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<ICreateLeaveTimeRequestValidator>();
            _mapperMock = new Mock<IDbLeaveTimeMapper>();
            _repositoryMock = new Mock<ILeaveTimeRepository>();
            _accessValidatorMock = new Mock<IAccessValidator>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var items = new Dictionary<object, object>
            {
                { "UserId", _createdBy }
            };

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContextAccessorMock
                .Setup(x => x.HttpContext.Items)
                .Returns(items);

            _accessValidatorMock
                .Setup(x => x.IsAdmin(null))
                .Returns(false);

            _command = new CreateLeaveTimeCommand(_validatorMock.Object, _mapperMock.Object, _repositoryMock.Object, _accessValidatorMock.Object, _httpContextAccessorMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenUserCreateLeaveTimeToOtherUserAndHisIsNotAdmin()
        {
            _accessValidatorMock
                .Setup(x => x.IsAdmin(null))
                .Returns(false);

            var items = new Dictionary<object, object>
            {
                { "UserId", Guid.NewGuid() }
            };

            _httpContextAccessorMock
                .Setup(x => x.HttpContext.Items)
                .Returns(items);

            Assert.Throws<ForbiddenException>(() => _command.Execute(_request));
            _repositoryMock.Verify(repository => repository.Add(It.IsAny<DbLeaveTime>()), Times.Never);
        }

        [Test]
        public void ShouldThrowExceptionWhenValidatorThrowsException()
        {
            _validatorMock
                .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                .Returns(false);

            Assert.Throws<ValidationException>(() => _command.Execute(_request));
            _repositoryMock.Verify(repository => repository.Add(It.IsAny<DbLeaveTime>()), Times.Never);
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
                .Setup(x => x.Add(It.IsAny<DbLeaveTime>()))
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
                .Setup(x => x.Add(It.IsAny<DbLeaveTime>()))
                .Returns(_createdLeaveTime.Id);

            var expected = new OperationResultResponse<Guid>
            {
                Body = _createdLeaveTime.Id,
                Status = OperationResultStatusType.FullSuccess
            };

            SerializerAssert.AreEqual(expected, _command.Execute(_request));
            _repositoryMock.Verify(repository => repository.Add(It.IsAny<DbLeaveTime>()), Times.Once);
        }
    }
}