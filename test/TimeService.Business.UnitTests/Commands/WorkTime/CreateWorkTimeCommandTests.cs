using FluentValidation;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces;
using LT.DigitalOffice.UnitTestKernel;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Business.UnitTests.Commands.WorkTime
{
    public class CreateWorkTimeCommandTests
    {
        private Mock<ICreateWorkTimeRequestValidator> _validatorMock;
        private Mock<IDbWorkTimeMapper> _mapperMock;
        private Mock<IWorkTimeRepository> _repositoryMock;
        private Mock<IAccessValidator> _accessValidatorMock;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private ICreateWorkTimeCommand _command;

        private CreateWorkTimeRequest _request;
        private DbWorkTime _createdWorkTime;
        private Guid _createdBy;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _createdBy = Guid.NewGuid();

            _request = new CreateWorkTimeRequest()
            {
                ProjectId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 7, 29, 9, 0, 0),
                EndTime = new DateTime(2020, 7, 29, 17, 0, 0),
                Title = "I was working on a very important task",
                Description = "I was asleep. I love sleep. I hope I get paid for this.",
                UserId = _createdBy
            };

            _createdWorkTime = new DbWorkTime()
            {
                Id = Guid.NewGuid(),
                ProjectId = _request.ProjectId,
                StartTime = _request.StartTime,
                EndTime = _request.EndTime,
                Title = _request.Title,
                Description = _request.Description,
                UserId = _createdBy
            };
        }

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<ICreateWorkTimeRequestValidator>();
            _mapperMock = new Mock<IDbWorkTimeMapper>();
            _repositoryMock = new Mock<IWorkTimeRepository>();
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

            _command = new CreateWorkTimeCommand(_validatorMock.Object, _mapperMock.Object, _repositoryMock.Object, _accessValidatorMock.Object, _httpContextAccessorMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenUserCreateWorkTimeToOtherUserAndHisIsNotAdmin()
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

            _accessValidatorMock
                .Setup(x => x.IsAdmin(null))
                .Returns(false);

            Assert.Throws<ForbiddenException>(() => _command.Execute(_request));
            _repositoryMock.Verify(repository => repository.Create(It.IsAny<DbWorkTime>()), Times.Never);
        }

        //[Test]
        //public void ShouldThrowExceptionWhenValidatorThrowsException()
        //{
        //    _validatorMock
        //        .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
        //        .Returns(false);

        //    Assert.Throws<ValidationException>(() => _command.Execute(_request));
        //    _repositoryMock.Verify(repository => repository.Create(It.IsAny<DbWorkTime>()), Times.Never);
        //}

        [Test]
        public void ShouldThrowExceptionWhenRepositoryThrowsException()
        {
            _validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            _mapperMock
                .Setup(x => x.Map(It.IsAny<CreateWorkTimeRequest>(), _createdBy))
                .Returns(_createdWorkTime);

            _repositoryMock
                .Setup(x => x.Create(It.IsAny<DbWorkTime>()))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _command.Execute(_request));
        }

        [Test]
        public void ShouldCreateNewWorkTimeWhenDataIsValid()
        {
            _validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            _mapperMock
                .Setup(x => x.Map(It.IsAny<CreateWorkTimeRequest>(), _createdBy))
                .Returns(_createdWorkTime);

            _repositoryMock
                .Setup(x => x.Create(It.IsAny<DbWorkTime>()))
                .Returns(_createdWorkTime.Id);

            var expected = new OperationResultResponse<Guid>
            {
                Body = _createdWorkTime.Id,
                Status = OperationResultStatusType.FullSuccess
            };

            SerializerAssert.AreEqual(expected, _command.Execute(_request));
            _repositoryMock.Verify(repository => repository.Create(It.IsAny<DbWorkTime>()), Times.Once);
        }
    }
}