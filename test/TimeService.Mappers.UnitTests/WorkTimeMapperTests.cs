using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto;
using LT.DigitalOffice.UnitTestKernel;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Mappers.UnitTests
{
    public class WorkTimeMapperTests
    {
        private IMapper<CreateWorkTimeRequest, DbWorkTime> createRequestMapper;
        private IMapper<EditWorkTimeRequest, DbWorkTime> editRequestMapper;

        private CreateWorkTimeRequest createRequest;
        private EditWorkTimeRequest editRequest;
        private DbWorkTime expectedDbWorkTimeWithoutId;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            createRequestMapper = new WorkTimeMapper();
            editRequestMapper = new WorkTimeMapper();
        }

        [SetUp]
        public void SetUp()
        {
            createRequest = new CreateWorkTimeRequest
            {
                ProjectId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 7, 29, 9, 0, 0),
                EndTime = new DateTime(2020, 7, 29, 9, 0, 0),
                Title = "I was working on a very important task",
                Description = "I was asleep. I love sleep. I hope I get paid for this.",
                WorkerUserId = Guid.NewGuid()
            };

            editRequest = new EditWorkTimeRequest
            {
                Id = Guid.NewGuid(),
                ProjectId = createRequest.ProjectId,
                StartTime = createRequest.StartTime,
                EndTime = createRequest.EndTime,
                Title = createRequest.Title,
                Description = createRequest.Description,
                WorkerUserId = createRequest.WorkerUserId
            };

            expectedDbWorkTimeWithoutId = new DbWorkTime
            {
                ProjectId = createRequest.ProjectId,
                StartTime = createRequest.StartTime,
                EndTime = createRequest.EndTime,
                Title = createRequest.Title,
                Description = createRequest.Description,
                WorkerUserId = createRequest.WorkerUserId
            };
        }

        #region CreateWorkTimeRequest to DbWorkTime
        [Test]
        public void ShouldThrowArgumentNullExceptionWhenCreateWorkTimeRequestIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => createRequestMapper.Map(null));
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidCreateWorkTimeRequest()
        {
            var newWorkTime = createRequestMapper.Map(createRequest);
            expectedDbWorkTimeWithoutId.Id = newWorkTime.Id;

            Assert.IsInstanceOf<Guid>(newWorkTime.Id);
            SerializerAssert.AreEqual(expectedDbWorkTimeWithoutId, newWorkTime);
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidCreateWorkTimeRequestWithNullDescription()
        {
            createRequest.Description = null;
            expectedDbWorkTimeWithoutId.Description = null;

            var newWortTime = createRequestMapper.Map(createRequest);
            expectedDbWorkTimeWithoutId.Id = newWortTime.Id;

            Assert.IsInstanceOf<Guid>(newWortTime.Id);
            Assert.IsTrue(string.IsNullOrEmpty(newWortTime.Description));
            SerializerAssert.AreEqual(expectedDbWorkTimeWithoutId, newWortTime);
        }
        #endregion

        #region EditWorkTimeRequest to DbWorkTime
        [Test]
        public void ShouldThrowArgumentNullExceptionWhenEditWorkTimeRequestIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => editRequestMapper.Map(null));
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidEditWorkTimeRequest()
        {
            var workTime = editRequestMapper.Map(editRequest);
            expectedDbWorkTimeWithoutId.Id = editRequest.Id;

            SerializerAssert.AreEqual(expectedDbWorkTimeWithoutId, workTime);
        }
        #endregion
    }
}