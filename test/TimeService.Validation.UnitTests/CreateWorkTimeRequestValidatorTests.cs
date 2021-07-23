using FluentValidation.TestHelper;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Validation.UnitTests
{
    public class CreateWorkTimeRequestValidatorTests
    {
        private Mock<IWorkTimeRepository> _repositoryMock;
        private ICreateWorkTimeRequestValidator _validator;

        private CreateWorkTimeRequest _request;
        private int _totalCount;
        private DbWorkTime _expectedDbWorkTime;

        //[SetUp]
        //public void Setup()
        //{
        //    _repositoryMock = new Mock<IWorkTimeRepository>();

        //    _validator = new CreateWorkTimeRequestValidator(_repositoryMock.Object);

        //    _request = new CreateWorkTimeRequest
        //    {
        //        UserId = Guid.NewGuid(),
        //        StartTime = DateTime.Now,
        //        EndTime = DateTime.Now.AddHours(6),
        //        Title = "Worked...",
        //        ProjectId = Guid.NewGuid(),
        //        Description = "Did something"
        //    };

        //    _expectedDbWorkTime = new DbWorkTime
        //    {
        //        UserId = _request.UserId,
        //        StartTime = _request.StartTime,
        //        EndTime = _request.EndTime,
        //        Title = _request.Title,
        //        ProjectId = _request.ProjectId,
        //        Description = _request.Description
        //    };

        //    _repositoryMock.Setup(x => x.Find(It.IsAny<FindWorkTimesFilter>(), It.IsAny<int>(), It.IsAny<int>(), out _totalCount))
        //        .Returns(new List<DbWorkTime> { _expectedDbWorkTime });
        //}

        //[Test]
        //public void ShouldNotHaveAnyValidationErrorsWhenRequestIsValid()
        //{
        //    _repositoryMock.Setup(x => x.Find(It.IsAny<FindWorkTimesFilter>(), It.IsAny<int>(), It.IsAny<int>(), out _totalCount))
        //        .Returns(new List<DbWorkTime>());

        //    _validator.TestValidate(_request).ShouldNotHaveAnyValidationErrors();
        //}

        //#region WorkerUserId
        //[Test]
        //public void ShouldHaveValidationErrorWhenWorkerUserIdIsEmpty()
        //{
        //    _request.UserId = Guid.Empty;
        //    _repositoryMock.Setup(x => x.Find(It.IsAny<FindWorkTimesFilter>(), It.IsAny<int>(), It.IsAny<int>(), out _totalCount))
        //        .Returns(new List<DbWorkTime>());

        //    _validator.TestValidate(_request).ShouldHaveValidationErrorFor(r => r.UserId);
        //}
        //#endregion

        //#region StartTime
        //[Test]
        //public void ShouldHaveValidationErrorWhenStartTimeTooEarly()
        //{
        //    var startTime = DateTime.Now.AddDays(CreateWorkTimeRequestValidator.ToDay).AddHours(1);
        //    _repositoryMock.Setup(x => x.Find(It.IsAny<FindWorkTimesFilter>(), It.IsAny<int>(), It.IsAny<int>(), out _totalCount))
        //        .Returns(new List<DbWorkTime>());

        //    _validator.ShouldHaveValidationErrorFor(x => x.StartTime, startTime);
        //}

        //[Test]
        //public void ShouldHaveValidationErrorWhenStartTimeTooLate()
        //{
        //    var startTime = DateTime.Now.AddDays(CreateWorkTimeRequestValidator.FromDay).AddHours(-1);
        //    _repositoryMock.Setup(x => x.Find(It.IsAny<FindWorkTimesFilter>(), It.IsAny<int>(), It.IsAny<int>(), out _totalCount))
        //        .Returns(new List<DbWorkTime>());

        //    _validator.ShouldHaveValidationErrorFor(x => x.StartTime, startTime);
        //}
        //#endregion

        //#region Title
        //[Test]
        //public void ShouldHaveValidationErrorWhenTitleIsEmpty()
        //{
        //    _request.Title = string.Empty;
        //    _repositoryMock.Setup(x => x.Find(It.IsAny<FindWorkTimesFilter>(), It.IsAny<int>(), It.IsAny<int>(), out _totalCount))
        //        .Returns(new List<DbWorkTime>());

        //    _validator.TestValidate(_request).ShouldHaveValidationErrorFor(r => r.Title);
        //}
        //#endregion

        //#region ProjectId
        //[Test]
        //public void ShouldHaveValidationErrorWhenProjectIdIsEmpty()
        //{
        //    _request.ProjectId = Guid.Empty;
        //    _repositoryMock.Setup(x => x.Find(It.IsAny<FindWorkTimesFilter>(), It.IsAny<int>(), It.IsAny<int>(), out _totalCount))
        //        .Returns(new List<DbWorkTime>());

        //    _validator.TestValidate(_request).ShouldHaveValidationErrorFor(r => r.ProjectId);
        //}
        //#endregion

        //[Test]
        //public void ShouldHaveAnyValidationErrorWhenEndTimeEarlierThanStarTime()
        //{
        //    var temp = _request.StartTime;
        //    _request.StartTime = _request.EndTime;
        //    _request.EndTime = temp;

        //    _validator.TestValidate(_request).ShouldHaveAnyValidationError();
        //}

        //[Test]
        //public void ShouldHaveAnyValidationErrorWhenWorkTimeGreaterThanWorkingLimit()
        //{
        //    var tooManyMinutes = CreateWorkTimeRequestValidator.WorkingLimit.TotalMinutes + 1;
        //    _request.EndTime = _request.StartTime.AddMinutes(tooManyMinutes);

        //    _validator.TestValidate(_request).ShouldHaveAnyValidationError();
        //}

        //[Test]
        //public void ShouldNotHaveAnyValidationErrorsWhenRequestOverlapsWithOtherTime()
        //{

        //    var successfulRequest = new CreateWorkTimeRequest
        //    {
        //        UserId = _request.UserId,
        //        StartTime = _request.StartTime.AddHours(-6),
        //        EndTime = _request.StartTime.AddHours(-5.85),
        //        Title = _request.Title,
        //        ProjectId = _request.ProjectId,
        //        Description = _request.Description
        //    };

        //    _validator.TestValidate(successfulRequest).ShouldNotHaveAnyValidationErrors();
        //}

        //[Test]
        //public void ShouldHaveAnyValidationErrorWhenRequestHaveIntersectionWithTheStartTime()
        //{
        //    var failRequest = new CreateWorkTimeRequest
        //    {
        //        UserId = _request.UserId,
        //        StartTime = _request.StartTime.AddHours(-1),
        //        EndTime = _request.EndTime.AddHours(-1),
        //        Title = _request.Title,
        //        ProjectId = _request.ProjectId,
        //        Description = _request.Description
        //    };

        //    _validator.TestValidate(failRequest).ShouldHaveAnyValidationError();
        //}

        //[Test]
        //public void ShouldHaveAnyValidationErrorWhenRequestHaveIntersectionInsideTime()
        //{
        //    var failRequest = new CreateWorkTimeRequest
        //    {
        //        UserId = _request.UserId,
        //        StartTime = _request.StartTime.AddHours(1),
        //        EndTime = _request.EndTime.AddHours(-1),
        //        Title = _request.Title,
        //        ProjectId = _request.ProjectId,
        //        Description = _request.Description
        //    };

        //    _validator.TestValidate(failRequest).ShouldHaveAnyValidationError();
        //}

        //[Test]
        //public void ShouldHaveAnyValidationErrorWhenRequestHaveIntersectionWithTheEndTime()
        //{
        //    var failRequest = new CreateWorkTimeRequest
        //    {
        //        UserId = _request.UserId,
        //        StartTime = _request.StartTime.AddHours(1),
        //        EndTime = _request.EndTime.AddHours(1),
        //        Title = _request.Title,
        //        ProjectId = _request.ProjectId,
        //        Description = _request.Description
        //    };

        //    _validator.TestValidate(failRequest).ShouldHaveAnyValidationError();
        //}
    }
}