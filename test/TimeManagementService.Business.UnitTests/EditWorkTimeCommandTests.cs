using FluentValidation;
using FluentValidation.Results;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeManagementService.Business.UnitTests
{
    public class EditWorkTimeCommandTests
    {
        private Mock<IValidator<EditWorkTimeRequest>> validatorMock;
        private Mock<IWorkTimeRepository> repositoryMock;
        private IEditWorkTimeCommand command;

        private EditWorkTimeRequest request;
        private DbWorkTime editedWorkTime;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            request = new EditWorkTimeRequest
            {
                WorkTimeId = Guid.NewGuid(),
                Patch = new JsonPatchDocument<DbWorkTime>()
            };

            editedWorkTime = new DbWorkTime() {};
        }

        [SetUp]
        public void SetUp()
        {
            validatorMock = new Mock<IValidator<EditWorkTimeRequest>>();
            repositoryMock = new Mock<IWorkTimeRepository>();

            validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            repositoryMock
                .Setup(x => x.GetWorkTime(It.IsAny<Guid>()))
                .Returns(new DbWorkTime());

            repositoryMock
                .Setup(x => x.EditWorkTime(It.IsAny<DbWorkTime>()))
                .Returns(true);

            command = new EditWorkTimeCommand(validatorMock.Object, repositoryMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenValidatorThrowsException()
        {
            validatorMock
                .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                .Returns(false);

            Assert.Throws<ValidationException>(() => command.Execute(request));
            repositoryMock.Verify(repository => repository.EditWorkTime(It.IsAny<DbWorkTime>()), Times.Never);
        }

        [Test]
        public void ShouldThrowExceptionWhenRepositoryThrowsException()
        {
            repositoryMock
                .Setup(x => x.EditWorkTime(It.IsAny<DbWorkTime>()))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => command.Execute(request));
        }

        [Test]
        public void ShouldEditNewWorkTimeWhenDataIsValid()
        {
            Assert.AreEqual(true, command.Execute(request));
            repositoryMock.Verify(repository => repository.EditWorkTime(It.IsAny<DbWorkTime>()), Times.Once);
        }
    }
}