using LT.DigitalOffice.TimeService.Mappers.Requests;
using LT.DigitalOffice.TimeService.Mappers.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.UnitTestKernel;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.UnitTests
{
    public class CreateLeaveTimeMapperTests
    {
        private ICreateLeaveTimeMapper _mapper;

        private CreateLeaveTimeRequest _request;
        private DbLeaveTime _expectedDbLeaveTimeWithoutId;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _mapper = new CreateLeaveTimeMapper();
        }

        [SetUp]
        public void SetUp()
        {
            _request = new CreateLeaveTimeRequest
            {
                LeaveType = LeaveType.SickLeave,
                Comment = "I have a sore throat",
                StartTime = new DateTime(2020, 7, 24),
                EndTime = new DateTime(2020, 7, 27),
                WorkerUserId = Guid.NewGuid()
            };

            _expectedDbLeaveTimeWithoutId = new DbLeaveTime
            {
                LeaveType = (int)_request.LeaveType,
                Comment = _request.Comment,
                StartTime = _request.StartTime,
                EndTime = _request.EndTime,
                WorkerUserId = _request.WorkerUserId
            };
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionWhenCreateLeaveTimeRequestIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _mapper.Map(null));
        }

        [Test]
        public void ShouldReturnDbLeaveTimeWhenMappingValidCreateLeaveTimeRequest()
        {
            var newLeaveTime = _mapper.Map(_request);
            _expectedDbLeaveTimeWithoutId.Id = newLeaveTime.Id;

            SerializerAssert.AreEqual(_expectedDbLeaveTimeWithoutId, newLeaveTime);
        }
    }
}