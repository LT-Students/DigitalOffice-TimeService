using FluentValidation.TestHelper;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Validation.LeaveTime;

namespace LT.DigitalOffice.TimeService.Validation.UnitTests
{
    public class CreateLeaveTimeRequestValidatorTests
    {
        private Mock<ILeaveTimeRepository> _repositoryMock;
        private ICreateLeaveTimeRequestValidator _validator;

        private CreateLeaveTimeRequest _request;
        private DbLeaveTime _expectedDbLeaveTime;
        private int _totalCount;

        /*[SetUp]
        public void Setup()
        {
            _repositoryMock = new Mock<ILeaveTimeRepository>();

            _validator = new CreateLeaveTimeRequestValidator(_repositoryMock.Object);

            _request = new CreateLeaveTimeRequest
            {
                LeaveType = LeaveType.SickLeave,
                Comment = "I have a sore throat",
                StartTime = new DateTime(2020, 7, 24),
                EndTime = new DateTime(2020, 7, 27),
                UserId = Guid.NewGuid()
            };

            _expectedDbLeaveTime = new DbLeaveTime
            {
                UserId = _request.UserId,
                StartTime = _request.StartTime,
                EndTime = _request.EndTime,
                Comment = _request.Comment,
                LeaveType = (int)_request.LeaveType
            };

            _repositoryMock.Setup(x => x.Find(It.IsAny<FindLeaveTimesFilter>(), 0, It.IsAny<int>(), out _totalCount))
                .Returns(new List<DbLeaveTime> { _expectedDbLeaveTime });
        }

        [Test]
        public void ShouldNotHaveAnyValidationErrorsWhenRequestIsValid()
        {
            _repositoryMock.Setup(x => x.Find(It.IsAny<FindLeaveTimesFilter>(), 0, It.IsAny<int>(), out _totalCount))
                .Returns(new List<DbLeaveTime>());

            _validator.TestValidate(_request).ShouldNotHaveAnyValidationErrors();
        }

        #region WorkerUserId
        [Test]
        public void ShouldHaveValidationErrorWhenWorkerUserIdIsEmpty()
        {
            var userId = Guid.Empty;
            _repositoryMock.Setup(x => x.Find(It.IsAny<FindLeaveTimesFilter>(), 0, It.IsAny<int>(), out _totalCount))
                .Returns(new List<DbLeaveTime>());

            _validator.ShouldHaveValidationErrorFor(x => x.UserId, userId);
        }
        #endregion

        #region StartTime
        [Test]
        public void ShouldHaveValidationErrorWhenStartTimeIsEqualToDefaultDateTime()
        {
            var startTime = default(DateTime);
            _repositoryMock.Setup(x => x.Find(It.IsAny<FindLeaveTimesFilter>(), 0, It.IsAny<int>(), out _totalCount))
                .Returns(new List<DbLeaveTime>());

            _validator.ShouldHaveValidationErrorFor(x => x.StartTime, startTime);
        }
        #endregion

        #region EndTime
        [Test]
        public void ShouldHaveValidationErrorWhenEndTimeIsEqualToDefaultDateTime()
        {
            var endTime = default(DateTime);
            _repositoryMock.Setup(x => x.Find(It.IsAny<FindLeaveTimesFilter>(), 0, It.IsAny<int>(), out _totalCount))
                .Returns(new List<DbLeaveTime>());

            _validator.ShouldHaveValidationErrorFor(x => x.EndTime, endTime);
        }
        #endregion

        #region LeaveType
        [Test]
        public void ShouldHaveValidationErrorWhenLeaveTypeIsNotInEnum()
        {
            const LeaveType leaveType = (LeaveType) 4;
            _repositoryMock.Setup(x => x.Find(It.IsAny<FindLeaveTimesFilter>(), 0, It.IsAny<int>(), out _totalCount))
                .Returns(new List<DbLeaveTime>());

            _validator.ShouldHaveValidationErrorFor(x => x.LeaveType, leaveType);
        }
        #endregion

        [Test]
        public void ShouldNotHaveAnyValidationErrorsWhenRequestOverlapWithOtherTime()
        {
            var successfulRequest = new CreateLeaveTimeRequest
            {
                UserId = _request.UserId,
                StartTime = _request.StartTime.AddHours(-6),
                EndTime = _request.StartTime.AddHours(-5.85),
                Comment = _request.Comment,
                LeaveType = _request.LeaveType
            };

            _validator.TestValidate(successfulRequest).ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void ShouldHaveAnyValidationErrorWhenRequestHaveIntersectionWithTheStartTime()
        {
            var failRequest = new CreateLeaveTimeRequest
            {
                UserId = _request.UserId,
                StartTime = _request.StartTime.AddHours(-1),
                EndTime = _request.EndTime.AddHours(-1),
                Comment = _request.Comment,
                LeaveType = _request.LeaveType
            };

            _validator.TestValidate(failRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveAnyValidationErrorWhenRequestHaveIntersectionInsideTime()
        {
            var failRequest = new CreateLeaveTimeRequest
            {
                UserId = _request.UserId,
                StartTime = _request.StartTime.AddHours(1),
                EndTime = _request.EndTime.AddHours(-1),
                Comment = _request.Comment,
                LeaveType = _request.LeaveType
            };

            _validator.TestValidate(failRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveAnyValidationErrorWhenRequestHaveIntersectionWithTheEndTime()
        {
            var failRequest = new CreateLeaveTimeRequest
            {
                UserId = _request.UserId,
                StartTime = _request.StartTime.AddHours(1),
                EndTime = _request.EndTime.AddHours(1),
                Comment = _request.Comment,
                LeaveType = _request.LeaveType
            };

            _validator.TestValidate(failRequest).ShouldHaveAnyValidationError();
        }*/
    }
}