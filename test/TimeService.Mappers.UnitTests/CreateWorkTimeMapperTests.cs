using LT.DigitalOffice.TimeService.Mappers.Requests;
using LT.DigitalOffice.TimeService.Mappers.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto;
using LT.DigitalOffice.UnitTestKernel;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.UnitTests
{
    public class CreateWorkTimeMapperTests
    {
        private ICreateWorkTimeMapper _createRequestMapper;

        private CreateWorkTimeRequest _createRequest;
        private DbWorkTime _expectedDbWorkTimeWithoutId;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _createRequestMapper = new CreateWorkTimeMapper();
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
                WorkerUserId = Guid.NewGuid()
            };

            _expectedDbWorkTimeWithoutId = new DbWorkTime
            {
                ProjectId = _createRequest.ProjectId,
                StartTime = _createRequest.StartTime,
                EndTime = _createRequest.EndTime,
                Title = _createRequest.Title,
                Description = _createRequest.Description,
                WorkerUserId = _createRequest.WorkerUserId
            };
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionWhenCreateWorkTimeRequestIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _createRequestMapper.Map(null));
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidCreateWorkTimeRequest()
        {
            var newWorkTime = _createRequestMapper.Map(_createRequest);
            _expectedDbWorkTimeWithoutId.Id = newWorkTime.Id;

            Assert.IsInstanceOf<Guid>(newWorkTime.Id);
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
    }
}