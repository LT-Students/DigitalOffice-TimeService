using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeManagementService.Business.UnitTests
{
    public class GetUserLeaveTimesCommandTests
    {
        private Mock<IMapper<DbLeaveTime, LeaveTime>> mapperMock;
        private Mock<ILeaveTimeRepository> repositoryMock;
        private IGetUserLeaveTimesCommand command;

        private Guid userIdRequest;
        private DbLeaveTime dbLeaveTime1;
        private LeaveTime leaveTime1;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            userIdRequest = Guid.NewGuid();

            dbLeaveTime1 = new DbLeaveTime { Id = Guid.NewGuid() };
            leaveTime1 = new LeaveTime { Id = dbLeaveTime1.Id };
        }

        [SetUp]
        public void SetUp()
        {
            mapperMock = new Mock<IMapper<DbLeaveTime, LeaveTime>>();
            repositoryMock = new Mock<ILeaveTimeRepository>();

            repositoryMock
                .Setup(x => x.GetUserLeaveTimes(userIdRequest))
                .Returns(new List<DbLeaveTime> { dbLeaveTime1 });

            mapperMock
                .Setup(x => x.Map(dbLeaveTime1))
                .Returns(leaveTime1);

            command = new GetUserLeaveTimesCommand(repositoryMock.Object, mapperMock.Object);
        }

        [Test]
        public void ShouldReturnUserWorkTimesWhenRequestIsValid()
        {
            Assert.AreEqual(new List<LeaveTime> { leaveTime1 }, command.Execute(userIdRequest));
        }

        [Test]
        public void ShouldThrowExceptionWhenRepositoryThrowsIt()
        {
            repositoryMock.Setup(x => x.GetUserLeaveTimes(It.IsAny<Guid>())).Throws(new Exception());

            Assert.Throws<Exception>(() => command.Execute(userIdRequest));
        }

        [Test]
        public void ShouldThrowExceptionWhenMapperThrowsIt()
        {
            mapperMock.Setup(x => x.Map(It.IsAny<DbLeaveTime>())).Throws(new Exception());

            Assert.Throws<Exception>(() => command.Execute(userIdRequest));
        }
    }
}