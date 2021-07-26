//using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
//using LT.DigitalOffice.Kernel.Exceptions.Models;
//using LT.DigitalOffice.TimeService.Business.Commands.WorkTime;
//using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
//using LT.DigitalOffice.TimeService.Data.Interfaces;
//using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
//using LT.DigitalOffice.TimeService.Models.Db;
//using LT.DigitalOffice.TimeService.Models.Dto.Filters;
//using LT.DigitalOffice.TimeService.Models.Dto.Models;
//using Microsoft.AspNetCore.Http;
//using Moq;
//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace LT.DigitalOffice.TimeService.Business.UnitTests.Commands.WorkTime
//{
//    public class FindWorkTimesCommandTests
//    {
//        private Mock<IWorkTimeInfoMapper> _mapperMock;
//        private Mock<IWorkTimeRepository> _repositoryMock;
//        private Mock<IAccessValidator> _accessValidatorMock;
//        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
//        private IFindWorkTimesCommand _command;

//        private Dictionary<object, object> _items;

//        [OneTimeSetUp]
//        public void OneTimeSetUp()
//        {
//            _items = new Dictionary<object, object>
//            {
//                { "UserId", Guid.NewGuid() }
//            };

//            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
//            _httpContextAccessorMock
//                .Setup(x => x.HttpContext.Items)
//                .Returns(_items);
//        }

//        [SetUp]
//        public void SetUp()
//        {
//            _mapperMock = new Mock<IWorkTimeInfoMapper>();
//            _repositoryMock = new Mock<IWorkTimeRepository>();
//            _accessValidatorMock = new Mock<IAccessValidator>();

//            _command = new FindWorkTimesCommand(_mapperMock.Object, _repositoryMock.Object, _accessValidatorMock.Object, _httpContextAccessorMock.Object);
//        }

//        [Test]
//        public void ShouldThrowExceptionWhenUserFindOtherUsersAndHisIsNotAdmin()
//        {
//            var filter = new FindWorkTimesFilter { UserId = Guid.NewGuid() };

//            _accessValidatorMock
//                .Setup(x => x.IsAdmin(null))
//                .Returns(false);

//            Assert.Throws<ForbiddenException>(() => _command.Execute(filter, 0, int.MaxValue));
//        }

//        [Test]
//        public void ShouldThrowExceptionWhenRepositoryThrowsException()
//        {
//            object userId;
//            _items.TryGetValue("UserId", out userId);

//            var filter = new FindWorkTimesFilter { UserId = (Guid)userId };

//            int totalCount;

//            _repositoryMock
//                .Setup(x => x.Find(filter, 0, 123, out totalCount))
//                .Throws(new Exception());

//            Assert.Throws<Exception>(() => _command.Execute(filter, 0, 123));
//        }

//        [Test]
//        public void ShouldThrowExceptionWhenMapperThrowsException()
//        {
//            object userId;
//            _items.TryGetValue("UserId", out userId);

//            var filter = new FindWorkTimesFilter { UserId = (Guid)userId };

//            var dbWorkTime = new DbWorkTime
//            {
//                Id = Guid.NewGuid()
//            };

//            int totalCount;

//            _repositoryMock
//                .Setup(x => x.Find(filter, 0, 123, out totalCount))
//                .Returns(new List<DbWorkTime> { dbWorkTime });

//            _mapperMock
//                .Setup(x => x.Map(dbWorkTime))
//                .Throws(new Exception());

//            Assert.Throws<Exception>(() => _command.Execute(filter, 0, 123));
//        }

//        [Test]
//        public void ShouldFindWorkTimesWhenRequestIsValid()
//        {
//            object userId;
//            _items.TryGetValue("UserId", out userId);

//            var filter = new FindWorkTimesFilter { UserId = (Guid)userId };

//            var dbWorkTime = new DbWorkTime
//            {
//                Id = Guid.NewGuid()
//            };

//            int totalCount;

//            _repositoryMock
//                .Setup(x => x.Find(filter, 0, 123, out totalCount))
//                .Returns(new List<DbWorkTime> { dbWorkTime });

//            var workTimeInfo = new WorkTimeInfo
//            {
//                Id = dbWorkTime.Id
//            };

//            _mapperMock
//                .Setup(x => x.Map(dbWorkTime))
//                .Returns(workTimeInfo);

//            Assert.AreEqual(workTimeInfo, _command.Execute(filter, 0, 123).Body.First());
//        }
//    }
//}