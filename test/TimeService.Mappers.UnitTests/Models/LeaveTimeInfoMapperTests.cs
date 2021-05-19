using LT.DigitalOffice.TimeService.Mappers.Models;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.UnitTestKernel;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.UnitTests.Models
{
    public class LeaveTimeInfoMapperTests
    {
        private ILeaveTimeInfoMapper _mapper;

        private DbLeaveTime _dbLeaveTime;
        private LeaveTimeInfo _expectedLeaveTimeInfo;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _mapper = new LeaveTimeInfoMapper();
        }

        [SetUp]
        public void SetUp()
        {
            _dbLeaveTime = new DbLeaveTime
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CreatedBy = Guid.NewGuid(),
                Comment = "comment",
                LeaveType = 1,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            _expectedLeaveTimeInfo = new LeaveTimeInfo
            {
                Id = _dbLeaveTime.Id,
                UserId = _dbLeaveTime.UserId,
                CreatedBy = _dbLeaveTime.CreatedBy,
                Comment = _dbLeaveTime.Comment,
                LeaveType = (LeaveType)_dbLeaveTime.LeaveType,
                StartTime = _dbLeaveTime.StartTime,
                EndTime = _dbLeaveTime.EndTime,
                CreatedAt = _dbLeaveTime.CreatedAt
            };
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionWhenDbLeaveTimeIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _mapper.Map(null));
        }

        [Test]
        public void ShouldReturnLeaveTimeInfoWhenMappingValidDbLeaveTime()
        {
            var result = _mapper.Map(_dbLeaveTime);

            SerializerAssert.AreEqual(_expectedLeaveTimeInfo, result);
        }
    }
}
