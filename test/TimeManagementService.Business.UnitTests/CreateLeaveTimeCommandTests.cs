using FluentValidation;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Enums;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using Moq;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Business.UnitTests
{
    public class CreateLeaveTimeCommandTests
    {
        private Mock<IValidator<LeaveTimeRequest>> validatorMock;
        private Mock<IMapper<LeaveTimeRequest, DbLeaveTime>> mapperMock;
        private Mock<ILeaveTimeRepository> repositoryMock;
        private ICreateLeaveTimeCommand command;

        private LeaveTimeRequest request;
        private DbLeaveTime createdLeaveTime;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            request = new LeaveTimeRequest()
            {
                LeaveType = LeaveType.SickLeave,
                Comment = "I have a sore throat",
                StartTime = new DateTime(2020, 7, 24),
                EndTime = new DateTime(2020, 7, 27),
                UserId = Guid.NewGuid()
            };

            createdLeaveTime = new DbLeaveTime()
            {
                Id = Guid.NewGuid(),
                LeaveType = (int)request.LeaveType,
                Comment = request.Comment,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                UserId = (Guid)request.UserId
            };
        }

        [SetUp]
        public void SetUp()
        {
            validatorMock = new Mock<IValidator<LeaveTimeRequest>>();
            mapperMock = new Mock<IMapper<LeaveTimeRequest, DbLeaveTime>>();
            repositoryMock = new Mock<ILeaveTimeRepository>();

            command = new CreateLeaveTimeCommand(validatorMock.Object, mapperMock.Object, repositoryMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenValidatorThrowsException()
        {
            validatorMock
                .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                .Returns(false);

            Assert.Throws<ValidationException>(() => command.Execute(request, Guid.NewGuid()));
            repositoryMock.Verify(repository => repository.CreateLeaveTime(It.IsAny<DbLeaveTime>()), Times.Never);
        }

        [Test]
        public void ShouldThrowExceptionWhenRepositoryThrowsException()
        {
            validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            mapperMock
                .Setup(x => x.Map(It.IsAny<LeaveTimeRequest>()))
                .Returns(createdLeaveTime);

            repositoryMock
                .Setup(x => x.CreateLeaveTime(It.IsAny<DbLeaveTime>()))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => command.Execute(request, Guid.NewGuid()));
        }

        [Test]
        public void ShouldCreateNewLeaveTimeWhenDataIsValid()
        {
            validatorMock
                 .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
                 .Returns(true);

            mapperMock
                .Setup(x => x.Map(It.IsAny<LeaveTimeRequest>()))
                .Returns(createdLeaveTime);

            repositoryMock
                .Setup(x => x.CreateLeaveTime(It.IsAny<DbLeaveTime>()))
                .Returns(createdLeaveTime.Id);

            Assert.AreEqual(createdLeaveTime.Id, command.Execute(request, Guid.NewGuid()));
            repositoryMock.Verify(repository => repository.CreateLeaveTime(It.IsAny<DbLeaveTime>()), Times.Once);
        }
    }
}