using LT.DigitalOffice.TimeService.Mappers.Db;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.UnitTestKernel;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.UnitTests
{
    public class DbWorkTimeMapperTests
    {
        private IDbWorkTimeMapper _createRequestMapper;
        private IDbWorkTimeMapper _editRequestMapper;

        private EditWorkTimeRequest _editRequest;
        private CreateWorkTimeRequest _createRequest;
        private DbWorkTime _editedDbWorkTime;
        private DbWorkTime _expectedCreatedDbWorkTimeWithoutId;
        private DbWorkTime _expectedEditedDbWorkTime;
        private Guid _createdBy;

        private void CreateDbWorkTimeSetUp()
        {
            _createRequest = new CreateWorkTimeRequest
            {
                ProjectId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 7, 29, 9, 0, 0),
                EndTime = new DateTime(2020, 7, 29, 9, 0, 0),
                Title = "I was working on a very important task",
                Description = "I was asleep. I love sleep. I hope I get paid for this.",
                UserId = Guid.NewGuid()
            };

            _expectedCreatedDbWorkTimeWithoutId = new DbWorkTime
            {
                ProjectId = _createRequest.ProjectId,
                CreatedBy = _createdBy,
                StartTime = _createRequest.StartTime,
                EndTime = _createRequest.EndTime,
                Title = _createRequest.Title,
                Description = _createRequest.Description,
                UserId = _createRequest.UserId
            };
        }

        private void EditDbWorkTimeSetUp()
        {
            _editedDbWorkTime = new DbWorkTime
            {
                Id = Guid.NewGuid(),
                CreatedBy = _createdBy,
                CreatedAt = DateTime.Now
            };

            _editRequest = new EditWorkTimeRequest
            {
                Id = _editedDbWorkTime.Id,
                ProjectId = _createRequest.ProjectId,
                StartTime = _createRequest.StartTime,
                EndTime = _createRequest.EndTime,
                Title = _createRequest.Title,
                Description = _createRequest.Description,
                UserId = _createRequest.UserId
            };

            _expectedEditedDbWorkTime = new DbWorkTime
            {
                Id = _editedDbWorkTime.Id,
                ProjectId = _createRequest.ProjectId,
                CreatedBy = _createdBy,
                StartTime = _createRequest.StartTime,
                EndTime = _createRequest.EndTime,
                CreatedAt = _editedDbWorkTime.CreatedAt,
                Title = _createRequest.Title,
                Description = _createRequest.Description,
                UserId = _createRequest.UserId
            };
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _createRequestMapper = new DbWorkTimeMapper();
            _editRequestMapper = new DbWorkTimeMapper();
        }

        [SetUp]
        public void SetUp()
        {
            _createdBy = Guid.NewGuid();

            CreateDbWorkTimeSetUp();
            EditDbWorkTimeSetUp();
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionWhenCreateWorkTimeRequestIsNull()
        {
            CreateWorkTimeRequest createWorkTimeRequest = null;
            Assert.Throws<ArgumentNullException>(() => _createRequestMapper.Map(createWorkTimeRequest, _createdBy));
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidCreateWorkTimeRequest()
        {
            var newDbWorkTime = _createRequestMapper.Map(_createRequest, _createdBy);
            _expectedCreatedDbWorkTimeWithoutId.Id = newDbWorkTime.Id;
            _expectedCreatedDbWorkTimeWithoutId.CreatedAt = newDbWorkTime.CreatedAt;

            SerializerAssert.AreEqual(_expectedCreatedDbWorkTimeWithoutId, newDbWorkTime);
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidCreateWorkTimeRequestWithNullDescription()
        {
            _createRequest.Description = null;
            _expectedCreatedDbWorkTimeWithoutId.Description = null;

            var newWortTime = _createRequestMapper.Map(_createRequest, _createdBy);
            _expectedCreatedDbWorkTimeWithoutId.Id = newWortTime.Id;
            _expectedCreatedDbWorkTimeWithoutId.CreatedAt = newWortTime.CreatedAt;

            Assert.IsInstanceOf<Guid>(newWortTime.Id);
            Assert.IsTrue(string.IsNullOrEmpty(newWortTime.Description));
            SerializerAssert.AreEqual(_expectedCreatedDbWorkTimeWithoutId, newWortTime);
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionWhenEditWorkTimeRequestIsNull()
        {
            EditWorkTimeRequest editWorkTimeRequest = null;
            Assert.Throws<ArgumentNullException>(() => _editRequestMapper.Map(editWorkTimeRequest, _editedDbWorkTime));
        }

        [Test]
        public void ShouldReturnDbWorkTimeWhenMappingValidEditWorkTimeRequest()
        {
            var dbWorkTime = _editRequestMapper.Map(_editRequest, _editedDbWorkTime);

            SerializerAssert.AreEqual(_expectedEditedDbWorkTime, dbWorkTime);
        }
    }
}