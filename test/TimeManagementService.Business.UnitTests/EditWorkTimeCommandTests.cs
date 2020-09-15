using FluentValidation;
using FluentValidation.Results;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeManagementService.Business.UnitTests
{
    public class EditWorkTimeCommandTests
    {
        private Mock<IValidator<EditWorkTimeRequest>> validatorMock;
        private Mock<IMapper<EditWorkTimeRequest, DbWorkTime>> mapperMock;
        private Mock<IWorkTimeRepository> repositoryMock;
        private IEditWorkTimeCommand command;

        private EditWorkTimeRequest request;
        private DbWorkTime editedWorkTime;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            request = new EditWorkTimeRequest()
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 7, 29, 9, 0, 0),
                EndTime = new DateTime(2020, 7, 29, 17, 0, 0),
                Title = "I was working on a very important task",
                Description = "I was asleep. I love sleep. I hope I get paid for this.",
                WorkerUserId = Guid.NewGuid()
            };

            editedWorkTime = new DbWorkTime()
            {
                Id = request.Id,
                ProjectId = Guid.NewGuid(),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Title = "I was working on a very very important task",
                Description = request.Description,
                WorkerUserId = request.WorkerUserId
            };
        }

        [SetUp]
        public void SetUp()
        {
            validatorMock = new Mock<IValidator<EditWorkTimeRequest>>();
            mapperMock = new Mock<IMapper<EditWorkTimeRequest, DbWorkTime>>();
            repositoryMock = new Mock<IWorkTimeRepository>();

            command = new EditWorkTimeCommand(validatorMock.Object, repositoryMock.Object, mapperMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenValidatorThrowsException()
        {
            validatorMock
                .Setup(x => x.Validate(It.IsAny<EditWorkTimeRequest>()))
                .Returns(new ValidationResult(
                    new List<ValidationFailure>
                    {
                        new ValidationFailure("test", "something", null)
                    }));

            Assert.Throws<ValidationException>(() => command.Execute(request));
            repositoryMock.Verify(repository => repository.EditWorkTime(It.IsAny<DbWorkTime>()), Times.Never);
        }

        [Test]
        public void ShouldThrowExceptionWhenRepositoryThrowsException()
        {
            validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            mapperMock
                .Setup(x => x.Map(It.IsAny<EditWorkTimeRequest>()))
                .Returns(editedWorkTime);

            repositoryMock
                .Setup(x => x.EditWorkTime(It.IsAny<DbWorkTime>()))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => command.Execute(request));
        }

        [Test]
        public void ShouldEditNewWorkTimeWhenDataIsValid()
        {
            validatorMock
                 .Setup(x => x.Validate(It.IsAny<EditWorkTimeRequest>()).IsValid)
                 .Returns(true);

            mapperMock
                .Setup(x => x.Map(It.IsAny<EditWorkTimeRequest>()))
                .Returns(editedWorkTime);

            repositoryMock
                .Setup(x => x.EditWorkTime(It.IsAny<DbWorkTime>()))
                .Returns(true);

            Assert.AreEqual(true, command.Execute(request));
            repositoryMock.Verify(repository => repository.EditWorkTime(It.IsAny<DbWorkTime>()), Times.Once);
        }
    }
}