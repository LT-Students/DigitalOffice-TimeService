using FluentValidation;
using FluentValidation.Results;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeManagementService.Business.UnitTests
{
    public class EditWorkTimeCommandTests
    {
        private Mock<IValidator<(JsonPatchDocument<WorkTime>, Guid)>> validatorMock;
        private Mock<IMapper<WorkTime, DbWorkTime>> mapperMock;
        private Mock<IWorkTimeRepository> repositoryMock;
        private IEditWorkTimeCommand command;

        private (Guid, JsonPatchDocument<DbWorkTime>) request;
        private DbWorkTime editedWorkTime;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            request = (Guid.NewGuid(), new JsonPatchDocument<DbWorkTime>());

            editedWorkTime = new DbWorkTime() {};
        }

        [SetUp]
        public void SetUp()
        {
            validatorMock = new Mock<IValidator<(JsonPatchDocument<WorkTime>, Guid)>>();
            mapperMock = new Mock<IMapper<WorkTime, DbWorkTime>>();
            repositoryMock = new Mock<IWorkTimeRepository>();

            command = new EditWorkTimeCommand(validatorMock.Object, repositoryMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenValidatorThrowsException()
        {
            validatorMock
                .Setup(x => x.Validate(It.IsAny<(JsonPatchDocument<WorkTime>, Guid)>()))
                .Returns(new ValidationResult(
                    new List<ValidationFailure>
                    {
                        new ValidationFailure("test", "something", null)
                    }));

            Assert.Throws<ValidationException>(() => command.Execute(request.Item1, request.Item2));
            repositoryMock.Verify(repository => repository.EditWorkTime(It.IsAny<DbWorkTime>()), Times.Never);
        }

        [Test]
        public void ShouldThrowExceptionWhenRepositoryThrowsException()
        {
            validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            mapperMock
                .Setup(x => x.Map(It.IsAny<WorkTime>()))
                .Returns(editedWorkTime);

            repositoryMock
                .Setup(x => x.EditWorkTime(It.IsAny<DbWorkTime>()))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => command.Execute(request.Item1, request.Item2));
        }

        [Test]
        public void ShouldEditNewWorkTimeWhenDataIsValid()
        {
            validatorMock
                 .Setup(x => x.Validate(It.IsAny<(JsonPatchDocument<WorkTime>, Guid)>()).IsValid)
                 .Returns(true);

            mapperMock
                .Setup(x => x.Map(It.IsAny<WorkTime>()))
                .Returns(editedWorkTime);

            repositoryMock
                .Setup(x => x.EditWorkTime(It.IsAny<DbWorkTime>()))
                .Returns(true);

            Assert.AreEqual(true, command.Execute(request.Item1, request.Item2));
            repositoryMock.Verify(repository => repository.EditWorkTime(It.IsAny<DbWorkTime>()), Times.Once);
        }
    }
}