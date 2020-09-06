using LT.DigitalOffice.TimeManagementService.Database.Entities;
using LT.DigitalOffice.TimeManagementService.Mappers;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models;
using LT.DigitalOffice.TimeManagementServiceUnitTests.UnitTestLibrary;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementServiceUnitTests.Mappers
{
    class EditWorkTimeRequestToDbWorkTimeMapperTests
    {
        private IMapper<EditWorkTimeRequest, DbWorkTime> mapper;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            mapper = new WorkTimeMapper();
        }

        [Test]
        public void ShouldReturnNewDbWorkTimeWhenDataCorrect()
        {
            var id = Guid.NewGuid();
            var workerUserId = Guid.NewGuid();
            var startTime = new DateTime(2020, 1, 1, 1, 1, 1);
            var endTime = new DateTime(2020, 2, 2, 2, 2, 2);
            var title = "ExampleTitle";
            var projectId = Guid.NewGuid();
            var description = "ExampleDescription";

            var request = new EditWorkTimeRequest
            {
                Id = id,
                WorkerUserId = workerUserId,
                StartTime = startTime,
                EndTime = endTime,
                Title = title,
                ProjectId = projectId,
                Description = description
            };

            var result = mapper.Map(request);

            var time = new DbWorkTime()
            {
                Id = id,
                WorkerUserId = workerUserId,
                StartTime = startTime,
                EndTime = endTime,
                Title = title,
                ProjectId = projectId,
                Description = description
            };

            SerializerAssert.AreEqual(time, result);
        }

        [Test]
        public void ShouldThrowExceptionWhenRequestIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => mapper.Map(null));
        }
    }
}
