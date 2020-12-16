using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Filters;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Responses;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeManagementService.Business.UnitTests
{
    public class GetUserWorkTimesCommandTests
    {
        private Mock<IMapper<DbWorkTime, WorkTimeResponse>> mapperMock;
        private Mock<IWorkTimeRepository> repositoryMock;
        private IGetUserWorkTimesCommand command;

        private Guid userIdRequest;
        private WorkTimeFilter filterRequest;
        private DbWorkTime dbWorkTime1;
        private WorkTimeResponse workTime1;
        private DbWorkTime dbWorkTime2;
        private WorkTimeResponse workTime2;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            userIdRequest = Guid.NewGuid();
            filterRequest = new WorkTimeFilter();

            dbWorkTime1 = new DbWorkTime { Id = Guid.NewGuid() };
            workTime1 = new WorkTimeResponse { Id = dbWorkTime1.Id };
            dbWorkTime2 = new DbWorkTime { Id = Guid.NewGuid() };
            workTime2 = new WorkTimeResponse { Id = dbWorkTime2.Id };
        }

        [SetUp]
        public void SetUp()
        {
            mapperMock = new Mock<IMapper<DbWorkTime, WorkTimeResponse>>();
            repositoryMock = new Mock<IWorkTimeRepository>();

            repositoryMock
                .Setup(x => x.GetUserWorkTimes(userIdRequest, filterRequest))
                .Returns(new List<DbWorkTime> { dbWorkTime1 });
            repositoryMock
                .Setup(x => x.GetUserWorkTimes(userIdRequest, null))
                .Returns(new List<DbWorkTime> { dbWorkTime2 });

            mapperMock
                .Setup(x => x.Map(dbWorkTime1))
                .Returns(workTime1);

            mapperMock
               .Setup(x => x.Map(dbWorkTime2))
               .Returns(workTime2);

            command = new GetUserWorkTimesCommand(repositoryMock.Object, mapperMock.Object);
        }

        [Test]
        public void ShouldReturnUserWorkTimesWhenRequestIsValid()
        {
            Assert.AreEqual(new List<WorkTimeResponse> { workTime1 }, command.Execute(userIdRequest, filterRequest));
            Assert.AreEqual(new List<WorkTimeResponse> { workTime2 }, command.Execute(userIdRequest, null));
        }

        [Test]
        public void ShouldThrowExceptionWhenRepositoryThrowsIt()
        {
            repositoryMock.Setup(x => x.GetUserWorkTimes(It.IsAny<Guid>(), It.IsAny<WorkTimeFilter>())).Throws(new Exception());

            Assert.Throws<Exception>(() => command.Execute(userIdRequest, filterRequest));
        }

        [Test]
        public void ShouldThrowExceptionWhenMapperThrowsIt()
        {
            mapperMock.Setup(x => x.Map(It.IsAny<DbWorkTime>())).Throws(new Exception());

            Assert.Throws<Exception>(() => command.Execute(userIdRequest, filterRequest));
        }
    }
}