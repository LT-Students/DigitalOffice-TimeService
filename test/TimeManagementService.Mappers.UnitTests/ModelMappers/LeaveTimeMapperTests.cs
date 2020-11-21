using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.ModelMappers;
using LT.DigitalOffice.TimeManagementService.Mappers.ModelMappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Enums;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using LT.DigitalOffice.UnitTestKernel;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Mappers.UnitTests.ModelMappers
{
    public class LeaveTimeMapperTests
    {
        private ILeaveTimeMapper mapper;

        private LeaveTime leaveTime;
        private DbLeaveTime expectedDbLeaveTimeWithoutId;

        private DbLeaveTime dbLeaveTime;
        private LeaveTime expectedLeaveTime;

        #region SetUp
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            mapper = new LeaveTimeMapper();
        }

        [SetUp]
        public void SetUp()
        {
            leaveTime = new LeaveTime
            {
                LeaveType = LeaveType.SickLeave,
                Comment = "I have a sore throat",
                StartTime = new DateTime(2020, 7, 24),
                EndTime = new DateTime(2020, 7, 27),
                WorkerUserId = Guid.NewGuid()
            };

            expectedDbLeaveTimeWithoutId = new DbLeaveTime
            {
                LeaveType = (int)leaveTime.LeaveType,
                Comment = leaveTime.Comment,
                StartTime = leaveTime.StartTime,
                EndTime = leaveTime.EndTime,
                WorkerUserId = leaveTime.WorkerUserId
            };

            dbLeaveTime = new DbLeaveTime
            {
                Id = Guid.NewGuid(),
                LeaveType = (int)LeaveType.SickLeave,
                Comment = "I have a sore throat",
                StartTime = new DateTime(2020, 7, 24),
                EndTime = new DateTime(2020, 7, 27),
                WorkerUserId = Guid.NewGuid()
            };

            expectedLeaveTime = new LeaveTime
            {
                Id = dbLeaveTime.Id,
                LeaveType = (LeaveType)dbLeaveTime.LeaveType,
                Comment = dbLeaveTime.Comment,
                StartTime = dbLeaveTime.StartTime,
                EndTime = dbLeaveTime.EndTime,
                WorkerUserId = dbLeaveTime.WorkerUserId
            };
        }
        #endregion

        [Test]
        public void ShouldThrowArgumentNullExceptionWhenLeaveTimeIsNull()
        {
            LeaveTime leaveTime = null;
            Assert.Throws<ArgumentNullException>(() => mapper.Map(leaveTime));
        }

        [Test]
        public void ShouldReturnDbLeaveTimeWhenMappingValidLeaveTime()
        {
            var newLeaveTime = mapper.Map(leaveTime);
            expectedDbLeaveTimeWithoutId.Id = newLeaveTime.Id;

            Assert.IsInstanceOf<Guid>(newLeaveTime.Id);
            SerializerAssert.AreEqual(expectedDbLeaveTimeWithoutId, newLeaveTime);
        }
    }
}