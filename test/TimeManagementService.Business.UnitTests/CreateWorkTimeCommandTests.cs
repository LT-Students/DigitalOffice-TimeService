using FluentValidation;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using Moq;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Business.UnitTests
{
    public class CreateWorkTimeCommandTests
    {
        private Mock<IValidator<WorkTimeRequest>> validatorMock;
        private Mock<IMapper<WorkTimeRequest, DbWorkTime>> mapperMock;
        private Mock<IWorkTimeRepository> repositoryMock;
        private ICreateWorkTimeCommand command;

        private WorkTimeRequest request;
        private DbWorkTime createdWorkTime;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            request = new WorkTimeRequest()
            {
                ProjectId = Guid.NewGuid(),
                StartDate = new DateTime(2020, 7, 29, 9, 0, 0),
                EndDate = new DateTime(2020, 7, 29, 17, 0, 0),
                Title = "I was working on a very important task",
                Description = "I was asleep. I love sleep. I hope I get paid for this.",
                UserId = Guid.NewGuid()
            };

            createdWorkTime = new DbWorkTime()
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Title = request.Title,
                Description = request.Description,
                UserId = (Guid)request.UserId
            };
        }

        [SetUp]
        public void SetUp()
        {
            validatorMock = new Mock<IValidator<WorkTimeRequest>>();
            mapperMock = new Mock<IMapper<WorkTimeRequest, DbWorkTime>>();
            repositoryMock = new Mock<IWorkTimeRepository>();

            validatorMock
                .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                .Returns(true);

            mapperMock
                .Setup(x => x.Map(It.IsAny<WorkTimeRequest>()))
                .Returns(createdWorkTime);

            repositoryMock
                .Setup(x => x.CreateWorkTime(It.IsAny<DbWorkTime>()))
                .Returns(createdWorkTime.Id);

            command = new CreateWorkTimeCommand(validatorMock.Object, mapperMock.Object, repositoryMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenValidatorThrowsException()
        {
            validatorMock
                .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                .Returns(false);

            Assert.Throws<ValidationException>(() => command.Execute(request, Guid.NewGuid()));
            repositoryMock.Verify(repository => repository.CreateWorkTime(It.IsAny<DbWorkTime>()), Times.Never);
        }

        [Test]
        public void ShouldThrowExceptionWhenMapperThrowsException()
        {
            mapperMock
                .Setup(x => x.Map(It.IsAny<WorkTimeRequest>()))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => command.Execute(request, Guid.NewGuid()));
        }

        [Test]
        public void ShouldThrowExceptionWhenRepositoryThrowsException()
        {
            repositoryMock
                .Setup(x => x.CreateWorkTime(It.IsAny<DbWorkTime>()))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => command.Execute(request, Guid.NewGuid()));
        }

        [Test]
        public void ShouldCreateNewWorkTimeWhenDataIsValid()
        {
            Assert.AreEqual(createdWorkTime.Id, command.Execute(request, Guid.NewGuid()));
            repositoryMock.Verify(repository => repository.CreateWorkTime(It.IsAny<DbWorkTime>()), Times.Once);
        }
    }
}