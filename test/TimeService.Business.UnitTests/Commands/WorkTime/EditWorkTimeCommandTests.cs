using FluentValidation;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Models.Dto.Requests.HelpersModels;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using LT.DigitalOffice.UnitTestKernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Moq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Business.UnitTests.Commands.WorkTime
{
    public class EditWorkTimeCommandTests
    {
        private Mock<IEditWorkTimeRequestValidator> _validatorMock;
        private Mock<IPatchDbWorkTimeMapper> _mapperMock;
        private Mock<IWorkTimeRepository> _repositoryMock;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private Mock<IAccessValidator> _accessValidatorMock;
        private IEditWorkTimeCommand _command;

        private JsonPatchDocument<EditWorkTimeRequest> _request;
        private JsonPatchDocument<DbWorkTime> _dbWorkTimeJsonPatchDocument;
        private DbWorkTime _editedDbWorkTime;
        private Dictionary<object, object> _items;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _request = new JsonPatchDocument<EditWorkTimeRequest>(new List<Operation<EditWorkTimeRequest>>
                {
                    new Operation<EditWorkTimeRequest>(
                        "replace",
                        $"/{nameof(EditWorkTimeRequest.Title)}",
                        "",
                        "new title")
                }, new CamelCasePropertyNamesContractResolver());

            _dbWorkTimeJsonPatchDocument = new JsonPatchDocument<DbWorkTime>(new List<Operation<DbWorkTime>>
                {
                    new Operation<DbWorkTime>(
                        "replace",
                        $"/{nameof(EditWorkTimeRequest.Title)}",
                        "",
                        "new title")
                }, new CamelCasePropertyNamesContractResolver());

            _editedDbWorkTime = new DbWorkTime()
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                CreatedBy = Guid.NewGuid(),
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now,
                CreatedAt = DateTime.Now,
                Title = "I was working on a very very important task",
                Description = "Worked...",
                UserId = Guid.NewGuid()
            };
        }

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<IEditWorkTimeRequestValidator>();
            _mapperMock = new Mock<IPatchDbWorkTimeMapper>();
            _repositoryMock = new Mock<IWorkTimeRepository>();
            _accessValidatorMock = new Mock<IAccessValidator>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _items = new Dictionary<object, object>();

            _httpContextAccessorMock
                .Setup(x => x.HttpContext.Items)
                .Returns(_items);

            _repositoryMock
                .Setup(x => x.Get(_editedDbWorkTime.Id))
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

            Assert.Throws<ValidationException>(() => _command.Execute(_editedDbWorkTime.Id, _request));
            _repositoryMock.Verify(repository => repository.Edit(It.IsAny<DbWorkTime>(), It.IsAny<JsonPatchDocument<DbWorkTime>>()), Times.Never);
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
                .Setup(x => x.Map(It.IsAny<JsonPatchDocument<EditWorkTimeRequest>>()))
                .Returns(_dbWorkTimeJsonPatchDocument);

            _repositoryMock
                .Setup(x => x.Edit(It.IsAny<DbWorkTime>(), It.IsAny<JsonPatchDocument<DbWorkTime>>()))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _command.Execute(_editedDbWorkTime.Id, _request));
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

            Assert.Throws<ForbiddenException>(() => _command.Execute(_editedDbWorkTime.Id, _request));
        }

        [Test]
        public void ShouldEditNewWorkTimeWhenDataIsValid()
        {
            _accessValidatorMock
                .Setup(x => x.IsAdmin(null))
                .Returns(true);

            _items.Add("UserId", _editedDbWorkTime.CreatedBy);

            _validatorMock
                 .Setup(x => x.Validate(It.IsAny<EditWorkTimeModel>()).IsValid)
                 .Returns(true);

            _mapperMock
                .Setup(x => x.Map(It.IsAny<JsonPatchDocument<EditWorkTimeRequest>>()))
                .Returns(_dbWorkTimeJsonPatchDocument);

            _repositoryMock
                .Setup(x => x.Edit(It.IsAny<DbWorkTime>(), It.IsAny<JsonPatchDocument<DbWorkTime>>()))
                .Returns(true);

            var result = _command.Execute(_editedDbWorkTime.Id, _request);

            var expected = new OperationResultResponse<bool>
            {
                Body = true,
                Status = OperationResultStatusType.FullSuccess,
                Errors = new()
            };

            SerializerAssert.AreEqual(expected, result);
            _repositoryMock.Verify(repository => repository.Edit(It.IsAny<DbWorkTime>(), It.IsAny<JsonPatchDocument<DbWorkTime>>()), Times.Once);
        }
    }
}