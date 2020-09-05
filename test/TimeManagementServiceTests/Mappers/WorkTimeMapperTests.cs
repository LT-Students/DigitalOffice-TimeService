using LT.DigitalOffice.TimeManagementService.Database.Entities;
using LT.DigitalOffice.TimeManagementService.Mappers;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models;
using LT.DigitalOffice.TimeManagementServiceUnitTests.UnitTestLibrary;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementServiceUnitTests.Mappers
{
    public class WorkTimeMapperTests
    {
        private IMapper<CreateWorkTimeRequest, DbWorkTime> mapper;

        private CreateWorkTimeRequest request;
        private DbWorkTime expectedDbWorkTimeWithoutId;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            mapper = new WorkTimeMapper();
        }

        [SetUp]
        public void SetUp()
        {
            request = new CreateWorkTimeRequest
            {
                ProjectId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 7, 29, 9, 0, 0),
                EndTime = new DateTime(2020, 7, 29, 9, 0, 0),
                Title = "I was working on a very important task",
                Description = "I was asleep. I love sleep. I hope I get paid for this.",
                WorkerUserId = Guid.NewGuid()
            };

            expectedDbWorkTimeWithoutId = new DbWorkTime
            {
                ProjectId = request.ProjectId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Title = request.Title,
                Description = request.Description,
                WorkerUserId = request.WorkerUserId
            };
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionWhenCreateWorkTimeRequestIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => mapper.Map(null));
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidCreateWorkTimeRequest()
        {
            var newWorkTime = mapper.Map(request);
            expectedDbWorkTimeWithoutId.Id = newWorkTime.Id;

            Assert.IsInstanceOf<Guid>(newWorkTime.Id);
            SerializerAssert.AreEqual(expectedDbWorkTimeWithoutId, newWorkTime);
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidCreateWorkTimeRequestWithNullDescription()
        {
            request.Description = null;
            expectedDbWorkTimeWithoutId.Description = null;

            var newWortTime = mapper.Map(request);
            expectedDbWorkTimeWithoutId.Id = newWortTime.Id;

            Assert.IsInstanceOf<Guid>(newWortTime.Id);
            Assert.IsTrue(string.IsNullOrEmpty(newWortTime.Description));
            SerializerAssert.AreEqual(expectedDbWorkTimeWithoutId, newWortTime);
        }
    }
}