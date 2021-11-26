using LT.DigitalOffice.TimeService.Mappers.Db;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.UnitTestKernel;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.UnitTests
{
    public class DbLeaveTimeMapperTests
    {
        private IDbLeaveTimeMapper _mapper;

        private CreateLeaveTimeRequest _request;
        private DbLeaveTime _expectedDbLeaveTimeWithoutId;
        private Guid _createdBy;

        /*[OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _mapper = new DbLeaveTimeMapper();
        }*/

        [SetUp]
        public void SetUp()
        {
            _createdBy = Guid.NewGuid();

            _request = new CreateLeaveTimeRequest
            {
                LeaveType = LeaveType.SickLeave,
                Comment = "I have a sore throat",
                StartTime = new DateTime(2020, 7, 24),
                EndTime = new DateTime(2020, 7, 27),
                UserId = Guid.NewGuid()
            };

            _expectedDbLeaveTimeWithoutId = new DbLeaveTime
            {
                CreatedBy = _createdBy,
                LeaveType = (int)_request.LeaveType,
                Comment = _request.Comment,
                StartTime = _request.StartTime,
                EndTime = _request.EndTime,
                UserId = _request.UserId,
                IsActive = true
            };
        }

        /*[Test]
        public void ShouldThrowArgumentNullExceptionWhenCreateLeaveTimeRequestIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _mapper.Map(null, Guid.NewGuid()));
        }

        [Test]
        public void ShouldReturnDbLeaveTimeWhenMappingValidCreateLeaveTimeRequest()
        {
            var newDbLeaveTime = _mapper.Map(_request, _createdBy);
            _expectedDbLeaveTimeWithoutId.Id = newDbLeaveTime.Id;
            _expectedDbLeaveTimeWithoutId.CreatedAt = newDbLeaveTime.CreatedAt;

            SerializerAssert.AreEqual(_expectedDbLeaveTimeWithoutId, newDbLeaveTime);
        }*/
    }
}
