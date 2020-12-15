using LT.DigitalOffice.TimeManagementService.Mappers.ModelMappers;
using LT.DigitalOffice.TimeManagementService.Mappers.ModelMappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using LT.DigitalOffice.UnitTestKernel;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Mappers.UnitTests.ModelMappers
{
    public class WorkTimeMapperTests
    {
        #region Variable declaration
        private IWorkTimeMapper mapper;

        private Guid id = Guid.NewGuid();
        private Guid projectId = Guid.NewGuid();
        private DateTime startTime = new DateTime(2020, 7, 29, 9, 0, 0);
        private DateTime endTime = new DateTime(2020, 7, 29, 9, 0, 0);
        private const string title = "I was working on a very important task";
        private const string description = "I was asleep. I love sleep. I hope I get paid for this.";
        private Guid workerUserId = Guid.NewGuid();
        private Guid currentUserId = Guid.NewGuid();

        private WorkTime workTime;
        private DbWorkTime expectedDbWorkTimeWithoutId;

        private DbWorkTime dbWorkTime;
        private WorkTime expectedWorkTime;
        #endregion

        #region SetUp
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            mapper = new WorkTimeMapper();
        }

        [SetUp]
        public void SetUp()
        {
            WorkTimeToDbWorkTimeSetUp();
            DbWorkTimeToWorkTimeSetUp();
        }

        private void WorkTimeToDbWorkTimeSetUp()
        {
            workTime = new WorkTime
            {
                ProjectId = projectId,
                StartDate = startTime,
                EndDate = endTime,
                Title = title,
                Description = description,
                UserId = workerUserId,
                CurrentUserId = currentUserId
            };

            expectedDbWorkTimeWithoutId = new DbWorkTime
            {
                ProjectId = projectId,
                StartDate = startTime,
                EndDate = endTime,
                Title = title,
                Description = description,
                UserId = workerUserId
            };
        }

        private void DbWorkTimeToWorkTimeSetUp()
        {
            dbWorkTime = new DbWorkTime
            {
                Id = id,
                ProjectId = projectId,
                StartDate = startTime,
                EndDate = endTime,
                Title = title,
                Description = description,
                UserId = workerUserId
            };

            expectedWorkTime = new WorkTime
            {
                Id = id,
                ProjectId = projectId,
                StartDate = startTime,
                EndDate = endTime,
                Title = title,
                Description = description,
                UserId = workerUserId
            };
        }
        #endregion

        #region WorkTime to DbWorkTime
        [Test]
        public void ShouldThrowArgumentNullExceptionWhenWorkTimeIsNull()
        {
            WorkTime workTime = null;
            Assert.Throws<ArgumentNullException>(() => mapper.Map(workTime));
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidWorkTime1()
        {
            var newWorkTime = mapper.Map(workTime);
            expectedDbWorkTimeWithoutId.Id = newWorkTime.Id;

            Assert.IsInstanceOf<Guid>(newWorkTime.Id);
            SerializerAssert.AreEqual(expectedDbWorkTimeWithoutId, newWorkTime);
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidWorkTime2()
        {
            workTime.UserId = null;
            var newWorkTime = mapper.Map(workTime);
            expectedDbWorkTimeWithoutId.UserId = workTime.CurrentUserId;
            expectedDbWorkTimeWithoutId.Id = newWorkTime.Id;

            Assert.IsInstanceOf<Guid>(newWorkTime.Id);
            SerializerAssert.AreEqual(expectedDbWorkTimeWithoutId, newWorkTime);
        }
        #endregion

        #region DbWorkTime to WorkTime
        [Test]
        public void ShouldThrowArgumentNullExceptionWhenDbWorkTimeIsNull()
        {
            DbWorkTime workTime = null;
            Assert.Throws<ArgumentNullException>(() => mapper.Map(workTime));
        }

        [Test]
        public void ShouldReturnWorkTimeWhenMappingValidDbWorkTime()
        {
            var workTime = mapper.Map(dbWorkTime);

            SerializerAssert.AreEqual(expectedWorkTime, workTime);
        }
        #endregion
    }
}