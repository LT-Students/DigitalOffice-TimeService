using FluentValidation;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Business.UnitTests.Commands.WorkTime
{
    public class EditWorkTimeCommandTests
    {
        private Mock<IEditWorkTimeRequestValidator> _validatorMock;
        private Mock<IDbWorkTimeMapper> _mapperMock;
        private Mock<IWorkTimeRepository> _repositoryMock;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private Mock<IAccessValidator> _accessValidatorMock;
        private IEditWorkTimeCommand _command;

        private EditWorkTimeRequest _request;
        private DbWorkTime _editedDbWorkTime;
        private Dictionary<object, object> _items;

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
                UserId = Guid.NewGuid()
            };

            _editedDbWorkTime = new DbWorkTime()
            {
                Id = _request.Id,
                ProjectId = Guid.NewGuid(),
                CreatedBy = Guid.NewGuid(),
                StartTime = _request.StartTime,
                EndTime = _request.EndTime,
                CreatedAt = DateTime.Now,
                Title = "I was working on a very very important task",
                Description = _request.Description,
                UserId = _request.UserId
            };
        }

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<IEditWorkTimeRequestValidator>();
            _mapperMock = new Mock<IDbWorkTimeMapper>();
            _repositoryMock = new Mock<IWorkTimeRepository>();
            _accessValidatorMock = new Mock<IAccessValidator>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _items = new Dictionary<object, object>();

            _httpContextAccessorMock
                .Setup(x => x.HttpContext.Items)
                .Returns(_items);

            _repositoryMock
                .Setup(x => x.GetWorkTime(_editedDbWorkTime.Id))
                .Returns(_editedDbWorkTime);

            _command = new EditWorkTimeCommand(_validatorMock.Object, _repositoryMock.Object, _mapperMock.Object, _accessValidatorMock.Object, _httpContextAccessorMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenValidatorThrowsException()
        {
            _accessValidatorMock
                .Setup(x => x.IsAdmin(null))
                .Returns(true);

            _items.Add("UserId", _editedDbWorkTime.CreatedBy);

            _validatorMock
                .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                .Returns(false);

            Assert.Throws<ValidationException>(() => _command.Execute(_request));
            _repositoryMock.Verify(repository => repository.EditWorkTime(It.IsAny<DbWorkTime>()), Times.Never);
        }

        [Test]
        public void ShouldThrowExceptionWhenRepositoryThrowsException()
        {
            _accessValidatorMock
                .Setup(x => x.IsAdmin(null))
                .Returns(true);

            _items.Add("UserId", _editedDbWorkTime.CreatedBy);

            _validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            _mapperMock
                .Setup(x => x.Map(It.IsAny<EditWorkTimeRequest>(), _editedDbWorkTime))
                .Returns(_editedDbWorkTime);

            _repositoryMock
                .Setup(x => x.EditWorkTime(It.IsAny<DbWorkTime>()))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _command.Execute(_request));
        }

        [Test]
        public void ShouldEditNewWorkTimeWhenDataIsValid()
        {
            _accessValidatorMock
                .Setup(x => x.IsAdmin(null))
                .Returns(true);

            _items.Add("UserId", _editedDbWorkTime.CreatedBy);

            _validatorMock
                 .Setup(x => x.Validate(It.IsAny<EditWorkTimeRequest>()).IsValid)
                 .Returns(true);

            _mapperMock
                .Setup(x => x.Map(It.IsAny<EditWorkTimeRequest>(), _editedDbWorkTime))
                .Returns(_editedDbWorkTime);

            _repositoryMock
                .Setup(x => x.EditWorkTime(It.IsAny<DbWorkTime>()))
                .Returns(true);

            Assert.AreEqual(true, _command.Execute(_request));
            _repositoryMock.Verify(repository => repository.EditWorkTime(It.IsAny<DbWorkTime>()), Times.Once);
        }

        [Test]
        public void SholdThrowExceptionWhenUserHasNoRights()
        {
            _accessValidatorMock
                .Setup(x => x.IsAdmin(null))
                .Returns(false);

            _items.Add("UserId", Guid.NewGuid());

            _validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            Assert.Throws<ForbiddenException>(() => _command.Execute(_request));
        }
    }
}