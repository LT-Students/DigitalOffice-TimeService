 using FluentValidation;
using LT.DigitalOffice.TimeManagementService.Commands;
using LT.DigitalOffice.TimeManagementService.Commands.Interfaces;
using LT.DigitalOffice.TimeManagementService.Database.Entities;
using LT.DigitalOffice.TimeManagementService.Mappers;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models;
using LT.DigitalOffice.TimeManagementService.Repositories.Interfaces;
using LT.DigitalOffice.TimeManagementService.Validators;
using Moq;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementServiceUnitTests.Commands
{
    class EditWorkTimeCommandTests
    {
        private Mock<IWorkTimeRepository> repositoryMock;
        private IEditWorkTimeCommand command;
        private IValidator<EditWorkTimeRequest> validator;
        private IMapper<EditWorkTimeRequest, DbWorkTime> mapper;
        private EditWorkTimeRequest request;

        [SetUp]
        public void SetUp()
        {
            request = new EditWorkTimeRequest
            {
                Id = Guid.NewGuid(),
                WorkerUserId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 1, 1, 1, 1, 1),
                EndTime = new DateTime(2020, 1, 1, 2, 2, 2),
                Title = "ExampleTitle",
                ProjectId = Guid.NewGuid(),
                Description = "ExampleDescription"
            };

            repositoryMock = new Mock<IWorkTimeRepository>();
            mapper = new WorkTimeMapper();
            validator = new EditWorkTimeRequestValidator(repositoryMock.Object);

            command = new EditWorkTimeCommand(validator, repositoryMock.Object, mapper);
        }

        [Test]
        public void ShouldThrowsExceptionWhenDataIsInvalid()
        {
            var incorrectRequest = new EditWorkTimeRequest
            {
                Id = Guid.NewGuid(),
                WorkerUserId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 1, 1, 1, 1, 1),
                EndTime = new DateTime(2020, 2, 2, 2, 2, 2),
                Title = "E",
                ProjectId = Guid.NewGuid(),
                Description = "ExampleDescription"   
            };

            Assert.Throws<ValidationException>(() => command.Execute(incorrectRequest));
        }

        [Test]
        public void ShouldThrowExceptionWhenIdIsNotExist()
        {
            repositoryMock
                .Setup(x => x.EditWorkTime(It.IsAny<DbWorkTime>()))
                .Throws(new Exception("Work time with this Id does not exist."));

            Assert.Throws<Exception>(() => command.Execute(request));
        }

        [Test]
        public void ShouldCreateTimeWhenDataIsValid()
        {
            repositoryMock
                .Setup(x => x.EditWorkTime(It.IsAny<DbWorkTime>()))
                .Returns(true);

            Assert.IsTrue(command.Execute(request));
        }
    }
}
