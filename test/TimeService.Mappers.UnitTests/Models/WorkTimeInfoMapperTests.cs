using LT.DigitalOffice.TimeService.Mappers.Models;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.UnitTestKernel;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.UnitTests.Models
{
    public class WorkTimeInfoMapperTests
    {
        private IWorkTimeInfoMapper _mapper;

        private DbWorkTime _dbWorkTime;
        private WorkTimeInfo _expectedWorkTimeInfo;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _mapper = new WorkTimeInfoMapper();
        }

        [SetUp]
        public void SetUp()
        {
            _dbWorkTime = new DbWorkTime
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CreatedBy = Guid.NewGuid(),
                Title = "title",
                Description = "description",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            _expectedWorkTimeInfo = new WorkTimeInfo
            {
                Id = _dbWorkTime.Id,
                ProjectId = _dbWorkTime.ProjectId,
                UserId = _dbWorkTime.UserId,
                CreatedBy = _dbWorkTime.CreatedBy,
                Title = _dbWorkTime.Title,
                Description = _dbWorkTime.Description,
                StartTime = _dbWorkTime.StartTime,
                EndTime = _dbWorkTime.EndTime,
                CreatedAt = _dbWorkTime.CreatedAt
            };
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionWhenDbWorkTimeIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _mapper.Map(null));
        }

        [Test]
        public void ShouldReturnWorkTimeInfoWhenMappingValidDbWorkTime()
        {
            var result = _mapper.Map(_dbWorkTime);

            SerializerAssert.AreEqual(_expectedWorkTimeInfo, result);
        }
    }
}
