using LT.DigitalOffice.Kernel.UnitTestLibrary;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Enums;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Mappers.UnitTests
{
    public class LeaveTimeMapperTests
    {
        private IMapper<CreateLeaveTimeRequest, DbLeaveTime> mapper;

        private CreateLeaveTimeRequest request;
        private DbLeaveTime expectedDbLeaveTimeWithoutId;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            mapper = new LeaveTimeMapper();
        }

        [SetUp]
        public void SetUp()
        {
            request = new CreateLeaveTimeRequest
            {
                LeaveType = LeaveType.SickLeave,
                Comment = "I have a sore throat",
                StartTime = new DateTime(2020, 7, 24),
                EndTime = new DateTime(2020, 7, 27),
                WorkerUserId = Guid.NewGuid()
            };

            expectedDbLeaveTimeWithoutId = new DbLeaveTime
            {
                LeaveType = (int)request.LeaveType,
                Comment = request.Comment,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                WorkerUserId = request.WorkerUserId
            };
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionWhenCreateLeaveTimeRequestIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => mapper.Map(null));
        }

        [Test]
        public void ShouldReturnDbLeaveTimeWhenMappingValidCreateLeaveTimeRequest()
        {
            var newLeaveTime = mapper.Map(request);
            expectedDbLeaveTimeWithoutId.Id = newLeaveTime.Id;

            Assert.IsInstanceOf<Guid>(newLeaveTime.Id);
            SerializerAssert.AreEqual(expectedDbLeaveTimeWithoutId, newLeaveTime);
        }
    }
}