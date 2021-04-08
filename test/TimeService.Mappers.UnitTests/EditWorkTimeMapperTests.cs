using LT.DigitalOffice.TimeService.Mappers.Requests;
using LT.DigitalOffice.TimeService.Mappers.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto;
using LT.DigitalOffice.UnitTestKernel;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.UnitTests
{
    public class WorkTimeMapperTests
    {
        private IEditWorkTimeMapper _editRequestMapper;

        private EditWorkTimeRequest _editRequest;
        private DbWorkTime _expectedDbWorkTimeWithoutId;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _editRequestMapper = new EditWorkTimeMapper();
        }

        [SetUp]
        public void SetUp()
        {
            _editRequest = new EditWorkTimeRequest
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 7, 29, 9, 0, 0),
                EndTime = new DateTime(2020, 7, 29, 9, 0, 0),
                Title = "I was working on a very important task",
                Description = "I was asleep. I love sleep. I hope I get paid for this.",
                WorkerUserId = Guid.NewGuid()
            };

            _expectedDbWorkTimeWithoutId = new DbWorkTime
            {
                ProjectId = _editRequest.ProjectId,
                StartTime = _editRequest.StartTime,
                EndTime = _editRequest.EndTime,
                Title = _editRequest.Title,
                Description = _editRequest.Description,
                WorkerUserId = _editRequest.WorkerUserId
            };
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionWhenEditWorkTimeRequestIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _editRequestMapper.Map(null));
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidEditWorkTimeRequest()
        {
            var workTime = _editRequestMapper.Map(_editRequest);
            _expectedDbWorkTimeWithoutId.Id = _editRequest.Id;

            SerializerAssert.AreEqual(_expectedDbWorkTimeWithoutId, workTime);
        }
    }
}