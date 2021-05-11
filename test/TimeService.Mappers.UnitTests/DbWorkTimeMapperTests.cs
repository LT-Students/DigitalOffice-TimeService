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
        private IDbWorkTimeMapper _editRequestMapper;

        private EditWorkTimeRequest _editRequest;
        private CreateWorkTimeRequest _createRequest;
        private DbWorkTime _expectedDbWorkTimeWithoutId;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _createRequestMapper = new DbWorkTimeMapper();
            _editRequestMapper = new DbWorkTimeMapper();
        }

        [SetUp]
        public void SetUp()
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

            _editRequest = new EditWorkTimeRequest
            {
                Id = Guid.NewGuid(),
                ProjectId = _createRequest.ProjectId,
                StartTime = _createRequest.StartTime,
                EndTime = _createRequest.EndTime,
                Title = _createRequest.Title,
                Description = _createRequest.Description,
                UserId = _createRequest.UserId
            };

            _expectedDbWorkTimeWithoutId = new DbWorkTime
            {
                ProjectId = _createRequest.ProjectId,
                StartTime = _createRequest.StartTime,
                EndTime = _createRequest.EndTime,
                Title = _createRequest.Title,
                Description = _createRequest.Description,
                UserId = _createRequest.UserId
            };
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionWhenCreateWorkTimeRequestIsNull()
        {
            CreateWorkTimeRequest createWorkTimeRequest = null;
            Assert.Throws<ArgumentNullException>(() => _createRequestMapper.Map(createWorkTimeRequest));
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidCreateWorkTimeRequest()
        {
            var newWorkTime = _createRequestMapper.Map(_createRequest);
            _expectedDbWorkTimeWithoutId.Id = newWorkTime.Id;

            SerializerAssert.AreEqual(_expectedDbWorkTimeWithoutId, newWorkTime);
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidCreateWorkTimeRequestWithNullDescription()
        {
            _createRequest.Description = null;
            _expectedDbWorkTimeWithoutId.Description = null;

            var newWortTime = _createRequestMapper.Map(_createRequest);
            _expectedDbWorkTimeWithoutId.Id = newWortTime.Id;

            Assert.IsInstanceOf<Guid>(newWortTime.Id);
            Assert.IsTrue(string.IsNullOrEmpty(newWortTime.Description));
            SerializerAssert.AreEqual(_expectedDbWorkTimeWithoutId, newWortTime);
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionWhenEditWorkTimeRequestIsNull()
        {
            EditWorkTimeRequest editWorkTimeRequest = null;
            Assert.Throws<ArgumentNullException>(() => _editRequestMapper.Map(editWorkTimeRequest));
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidEditWorkTimeRequest()
        {
            var workTime = _editRequestMapper.Map(_editRequest);
            _expectedDbWorkTimeWithoutId.Id = _editRequest.Id;

            SerializerAssert.AreEqual(_expectedDbWorkTimeWithoutId, workTime);
        }
    }
}