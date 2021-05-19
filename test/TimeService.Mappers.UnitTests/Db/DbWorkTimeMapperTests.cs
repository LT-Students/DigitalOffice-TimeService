using LT.DigitalOffice.TimeService.Mappers.Db;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.UnitTestKernel;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.UnitTests
{
    public class DbWorkTimeMapperTests
    {
        private IDbWorkTimeMapper _createRequestMapper;

        private CreateWorkTimeRequest _createRequest;
        private DbWorkTime _expectedCreatedDbWorkTimeWithoutId;
        private Guid _createdBy;

        private void CreateDbWorkTimeSetUp()
        {
            _createRequest = new CreateWorkTimeRequest
            {
                ProjectId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 7, 29, 9, 0, 0),
                EndTime = new DateTime(2020, 7, 29, 9, 0, 0),
                Title = "I was working on a very important task",
                Description = "I was asleep. I love sleep. I hope I get paid for this.",
                UserId = Guid.NewGuid()
            };

            _expectedCreatedDbWorkTimeWithoutId = new DbWorkTime
            {
                ProjectId = _createRequest.ProjectId,
                CreatedBy = _createdBy,
                StartTime = _createRequest.StartTime,
                EndTime = _createRequest.EndTime,
                Title = _createRequest.Title,
                Description = _createRequest.Description,
                UserId = _createRequest.UserId
            };
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _createRequestMapper = new DbWorkTimeMapper();
        }

        [SetUp]
        public void SetUp()
        {
            _createdBy = Guid.NewGuid();

            CreateDbWorkTimeSetUp();
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionWhenCreateWorkTimeRequestIsNull()
        {
            CreateWorkTimeRequest createWorkTimeRequest = null;
            Assert.Throws<ArgumentNullException>(() => _createRequestMapper.Map(createWorkTimeRequest, _createdBy));
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidCreateWorkTimeRequest()
        {
            var newDbWorkTime = _createRequestMapper.Map(_createRequest, _createdBy);
            _expectedCreatedDbWorkTimeWithoutId.Id = newDbWorkTime.Id;
            _expectedCreatedDbWorkTimeWithoutId.CreatedAt = newDbWorkTime.CreatedAt;

            SerializerAssert.AreEqual(_expectedCreatedDbWorkTimeWithoutId, newDbWorkTime);
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidCreateWorkTimeRequestWithNullDescription()
        {
            _createRequest.Description = null;
            _expectedCreatedDbWorkTimeWithoutId.Description = null;

            var newWortTime = _createRequestMapper.Map(_createRequest, _createdBy);
            _expectedCreatedDbWorkTimeWithoutId.Id = newWortTime.Id;
            _expectedCreatedDbWorkTimeWithoutId.CreatedAt = newWortTime.CreatedAt;

            Assert.IsInstanceOf<Guid>(newWortTime.Id);
            Assert.IsTrue(string.IsNullOrEmpty(newWortTime.Description));
            SerializerAssert.AreEqual(_expectedCreatedDbWorkTimeWithoutId, newWortTime);
        }
    }
}