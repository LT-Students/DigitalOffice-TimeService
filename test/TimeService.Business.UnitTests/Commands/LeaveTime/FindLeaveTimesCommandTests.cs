using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Business.UnitTests.Commands.LeaveTime
{
    public class FindLeaveTimesCommandTests
    {
        private Mock<ILeaveTimeInfoMapper> _mapperMock;
        private Mock<ILeaveTimeRepository> _repositoryMock;
        private Mock<IAccessValidator> _accessValidatorMock;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private IFindLeaveTimesCommand _command;

        private Dictionary<object, object> _items;
        private Guid _authorId = Guid.NewGuid();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _items = new Dictionary<object, object>
            {
                { "UserId", _authorId }
            };

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContextAccessorMock
                .Setup(x => x.HttpContext.Items)
                .Returns(_items);
        }

        [SetUp]
        public void SetUp()
        {
            _mapperMock = new Mock<ILeaveTimeInfoMapper>();
            _repositoryMock = new Mock<ILeaveTimeRepository>();
            _accessValidatorMock = new Mock<IAccessValidator>();

            _command = new FindLeaveTimesCommand(_mapperMock.Object, _repositoryMock.Object, _accessValidatorMock.Object, _httpContextAccessorMock.Object);
        }

        [Test]
        public void ShouldThrowExceptionWhenUserFindOtherUsersAndHisIsNotAdmin()
        {
            var filter = new FindLeaveTimesFilter { UserId = Guid.NewGuid() };

            _accessValidatorMock
                .Setup(x => x.IsAdmin(null))
                .Returns(false);

            Assert.Throws<ForbiddenException>(() => _command.Execute(filter, 0, int.MaxValue));
        }

        [Test]
        public void ShouldThrowExceptionWhenRepositoryThrowsException()
        {
            var filter = new FindLeaveTimesFilter { UserId = _authorId };

            int totalCount;

            _repositoryMock
                .Setup(x => x.Find(filter, 0, 123, out totalCount))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _command.Execute(filter, 0, 123));
        }

        [Test]
        public void ShouldThrowExceptionWhenMapperThrowsException()
        {
            var filter = new FindLeaveTimesFilter { UserId = _authorId };

            var dbLeaveTime = new DbLeaveTime
            {
                Id = Guid.NewGuid()
            };

            int totalCount;

            _repositoryMock
                .Setup(x => x.Find(filter, 0, 123, out totalCount))
                .Returns(new List<DbLeaveTime> { dbLeaveTime });

            _mapperMock
                .Setup(x => x.Map(dbLeaveTime))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _command.Execute(filter, 0, 123));
        }

        [Test]
        public void ShouldFindLeaveTimesWhenRequestIsValid()
        {
            var filter = new FindLeaveTimesFilter { UserId = _authorId };

            var dbLeaveTime = new DbLeaveTime
            {
                Id = Guid.NewGuid()
            };

            int totalCount;

            _repositoryMock
                .Setup(x => x.Find(filter, 0, 123, out totalCount))
                .Returns(new List<DbLeaveTime> { dbLeaveTime });

            var leaveTimeInfo = new LeaveTimeInfo
            {
                Id = dbLeaveTime.Id
            };

            _mapperMock
                .Setup(x => x.Map(dbLeaveTime))
                .Returns(leaveTimeInfo);

            Assert.AreEqual(leaveTimeInfo, _command.Execute(filter, 0, 123).Body.First());
        }
    }
}