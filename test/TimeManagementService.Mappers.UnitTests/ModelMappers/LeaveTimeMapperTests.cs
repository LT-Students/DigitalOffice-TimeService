using LT.DigitalOffice.TimeManagementService.Mappers.ModelMappers;
using LT.DigitalOffice.TimeManagementService.Mappers.ModelMappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Enums;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Responses;
using LT.DigitalOffice.UnitTestKernel;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Mappers.UnitTests.ModelMappers
{
    public class LeaveTimeMapperTests
    {
        #region Variable declaration
        private ILeaveTimeMapper mapper;

        private Guid id = Guid.NewGuid();
        private const LeaveType leaveType = LeaveType.SickLeave;
        private const string comment = "I have a sore throat";
        private DateTime startTime = new DateTime(2020, 7, 24);
        private DateTime endTime = new DateTime(2020, 7, 27);
        private Guid userId = Guid.NewGuid();
        private Guid currentUserId = Guid.NewGuid();

        private LeaveTimeRequest leaveTime;
        private DbLeaveTime expectedDbLeaveTimeWithoutId;

        private DbLeaveTime dbLeaveTime;
        private LeaveTimeResponse expectedLeaveTime;
        #endregion

        #region SetUp
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            mapper = new LeaveTimeMapper();
        }

        [SetUp]
        public void SetUp()
        {
            LeaveTimeToDbLeaveTime();
            DbLeaveTimeToLeaveTime();
        }

        private void LeaveTimeToDbLeaveTime()
        {
            leaveTime = new LeaveTimeRequest
            {
                LeaveType = leaveType,
                Comment = comment,
                StartTime = startTime,
                EndTime = endTime,
                UserId = userId,
                CurrentUserId = currentUserId
            };

            expectedDbLeaveTimeWithoutId = new DbLeaveTime
            {
                LeaveType = (int)leaveType,
                Comment = comment,
                StartTime = startTime,
                EndTime = endTime,
                UserId = userId
            };
        }

        private void DbLeaveTimeToLeaveTime()
        {
            dbLeaveTime = new DbLeaveTime
            {
                Id = id,
                LeaveType = (int)leaveType,
                Comment = comment,
                StartTime = startTime,
                EndTime = endTime,
                UserId = userId
            };

            expectedLeaveTime = new LeaveTimeResponse
            {
                Id = id,
                LeaveType = leaveType,
                Comment = comment,
                StartTime = startTime,
                EndTime = endTime,
                UserId = userId
            };
        }
        #endregion

        #region LeaveTime to DbLeaveTime
        [Test]
        public void ShouldThrowArgumentNullExceptionWhenLeaveTimeIsNull()
        {
            LeaveTimeRequest leaveTime = null;
            Assert.Throws<ArgumentNullException>(() => mapper.Map(leaveTime));
        }

        [Test]
        public void ShouldReturnDbLeaveTimeWhenMappingValidLeaveTime1()
        {
            var resultDbLeaveTime = mapper.Map(leaveTime);
            expectedDbLeaveTimeWithoutId.Id = resultDbLeaveTime.Id;

            Assert.IsInstanceOf<Guid>(resultDbLeaveTime.Id);
            SerializerAssert.AreEqual(expectedDbLeaveTimeWithoutId, resultDbLeaveTime);
        }

        [Test]
        public void ShouldReturnDbLeaveTimeWhenMappingValidLeaveTime2()
        {
            leaveTime.UserId = null;
            var resultDbLeaveTime = mapper.Map(leaveTime);
            expectedDbLeaveTimeWithoutId.UserId = leaveTime.CurrentUserId;
            expectedDbLeaveTimeWithoutId.Id = resultDbLeaveTime.Id;

            Assert.IsInstanceOf<Guid>(resultDbLeaveTime.Id);
            SerializerAssert.AreEqual(expectedDbLeaveTimeWithoutId, resultDbLeaveTime);
        }
        #endregion

        #region DbLeaveTime to LeaveTime
        [Test]
        public void ShouldThrowArgumentNullExceptionWhenDbLeaveTimeIsNull()
        {
            DbLeaveTime dbLeaveTime = null;
            Assert.Throws<ArgumentNullException>(() => mapper.Map(dbLeaveTime));
        }

        [Test]
        public void ShouldReturnDbLeaveTimeWhenMappingValidDbLeaveTime()
        {
            var redultLeaveTime = mapper.Map(dbLeaveTime);

            SerializerAssert.AreEqual(expectedLeaveTime, redultLeaveTime);
        }
        #endregion
    }
}