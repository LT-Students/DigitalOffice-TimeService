using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
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
        private IWorkTimeMapper mapper;

        private WorkTime workTime;
        private DbWorkTime expectedDbWorkTimeWithoutId;

        private DbWorkTime dbWorkTime;
        private WorkTime expectedWorkTime;

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
                ProjectId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 7, 29, 9, 0, 0),
                EndTime = new DateTime(2020, 7, 29, 9, 0, 0),
                Title = "I was working on a very important task",
                Description = "I was asleep. I love sleep. I hope I get paid for this.",
                WorkerUserId = Guid.NewGuid()
            };

            expectedDbWorkTimeWithoutId = new DbWorkTime
            {
                ProjectId = workTime.ProjectId,
                StartTime = workTime.StartTime,
                EndTime = workTime.EndTime,
                Title = workTime.Title,
                Description = workTime.Description,
                WorkerUserId = workTime.WorkerUserId
            };
        }

        private void DbWorkTimeToWorkTimeSetUp()
        {
            dbWorkTime = new DbWorkTime
            {
                Id = new Guid(),
                ProjectId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 7, 29, 9, 0, 0),
                EndTime = new DateTime(2020, 7, 29, 9, 0, 0),
                Title = "I was working on a very important task",
                Description = "I was asleep. I love sleep. I hope I get paid for this.",
                WorkerUserId = Guid.NewGuid()
            };

            expectedWorkTime = new WorkTime
            {
                Id = dbWorkTime.Id,
                ProjectId = dbWorkTime.ProjectId,
                StartTime = dbWorkTime.StartTime,
                EndTime = dbWorkTime.EndTime,
                Title = dbWorkTime.Title,
                Description = dbWorkTime.Description,
                WorkerUserId = dbWorkTime.WorkerUserId
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
        public void ShouldReturnDbWorkTimeWhenMappingValidWorkTime()
        {
            var newWorkTime = mapper.Map(workTime);
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