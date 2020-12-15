using FluentValidation;
using FluentValidation.TestHelper;
using LT.DigitalOffice.TimeManagementService.Data.Filters;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeManagementService.Validation.UnitTests
{
    public class CreateWorkTimeRequestValidatorTests
    {
        private Mock<IWorkTimeRepository> repositoryMock;
        private IValidator<WorkTime> validator;

        private WorkTime request;
        private DbWorkTime expectedDbWorkTime;

        [SetUp]
        public void Setup()
        {
            repositoryMock = new Mock<IWorkTimeRepository>();

            validator = new WorkTimeValidator(repositoryMock.Object);

            request = new WorkTime
            {
                UserId = Guid.NewGuid(),
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(6),
                Title = "Worked...",
                ProjectId = Guid.NewGuid(),
                Description = "Did something"
            };

            expectedDbWorkTime = new DbWorkTime
            {
                UserId = request.UserId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Title = request.Title,
                ProjectId = request.ProjectId,
                Description = request.Description
            };

            repositoryMock.Setup(x => x.GetUserWorkTimes(request.UserId, It.IsAny<WorkTimeFilter>()))
                .Returns(new List<DbWorkTime> { expectedDbWorkTime });
        }

        [Test]
        public void ShouldNotHaveAnyValidationErrorsWhenRequestIsValid()
        {
            repositoryMock.Setup(x => x.GetUserWorkTimes(It.IsAny<Guid>(), It.IsAny<WorkTimeFilter>()))
                .Returns(new List<DbWorkTime>());

            validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
        }

        #region WorkerUserId
        [Test]
        public void ShouldHaveValidationErrorWhenWorkerUserIdIsEmpty()
        {
            request.UserId = Guid.Empty;
            repositoryMock.Setup(x => x.GetUserWorkTimes(It.IsAny<Guid>(), It.IsAny<WorkTimeFilter>()))
                .Returns(new List<DbWorkTime>());

            validator.TestValidate(request).ShouldHaveValidationErrorFor(r => r.UserId);
        }
        #endregion

        #region StartTime
        [Test]
        public void ShouldHaveValidationErrorWhenStartTimeTooEarly()
        {
            var startTime = DateTime.Now.AddDays(WorkTimeValidator.ToDay).AddHours(1);
            repositoryMock.Setup(x => x.GetUserWorkTimes(It.IsAny<Guid>(), It.IsAny<WorkTimeFilter>()))
                .Returns(new List<DbWorkTime>());

            validator.ShouldHaveValidationErrorFor(x => x.StartDate, startTime);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenStartTimeTooLate()
        {
            var startTime = DateTime.Now.AddDays(WorkTimeValidator.FromDay).AddHours(-1);
            repositoryMock.Setup(x => x.GetUserWorkTimes(It.IsAny<Guid>(), It.IsAny<WorkTimeFilter>()))
                .Returns(new List<DbWorkTime>());

            validator.ShouldHaveValidationErrorFor(x => x.StartDate, startTime);
        }
        #endregion

        #region Title
        [Test]
        public void ShouldHaveValidationErrorWhenTitleIsEmpty()
        {
            request.Title = string.Empty;
            repositoryMock.Setup(x => x.GetUserWorkTimes(It.IsAny<Guid>(), It.IsAny<WorkTimeFilter>()))
                .Returns(new List<DbWorkTime>());

            validator.TestValidate(request).ShouldHaveValidationErrorFor(r => r.Title);
        }
        #endregion

        #region ProjectId
        [Test]
        public void ShouldHaveValidationErrorWhenProjectIdIsEmpty()
        {
            request.ProjectId = Guid.Empty;
            repositoryMock.Setup(x => x.GetUserWorkTimes(It.IsAny<Guid>(), It.IsAny<WorkTimeFilter>()))
                .Returns(new List<DbWorkTime>());

            validator.TestValidate(request).ShouldHaveValidationErrorFor(r => r.ProjectId);
        }
        #endregion

        [Test]
        public void ShouldHaveAnyValidationErrorWhenEndTimeEarlierThanStarTime()
        {
            var temp = request.StartDate;
            request.StartDate = request.EndDate;
            request.EndDate = temp;

            validator.TestValidate(request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveAnyValidationErrorWhenWorkTimeGreaterThanWorkingLimit()
        {
            var tooManyMinutes = WorkTimeValidator.WorkingLimit.TotalMinutes + 1;
            request.EndDate = request.StartDate.AddMinutes(tooManyMinutes);

            validator.TestValidate(request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldNotHaveAnyValidationErrorsWhenRequestOverlapsWithOtherTime()
        {

            var successfulRequest = new WorkTime
            {
                UserId = request.UserId,
                StartDate = request.StartDate.AddHours(-6),
                EndDate = request.StartDate.AddHours(-5.85),
                Title = request.Title,
                ProjectId = request.ProjectId,
                Description = request.Description
            };

            validator.TestValidate(successfulRequest).ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void ShouldHaveAnyValidationErrorWhenRequestHaveIntersectionWithTheStartTime()
        {
            var failRequest = new WorkTime
            {
                UserId = request.UserId,
                StartDate = request.StartDate.AddHours(-1),
                EndDate = request.EndDate.AddHours(-1),
                Title = request.Title,
                ProjectId = request.ProjectId,
                Description = request.Description
            };

            validator.TestValidate(failRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveAnyValidationErrorWhenRequestHaveIntersectionInsideTime()
        {
            var failRequest = new WorkTime
            {
                UserId = request.UserId,
                StartDate = request.StartDate.AddHours(1),
                EndDate = request.EndDate.AddHours(-1),
                Title = request.Title,
                ProjectId = request.ProjectId,
                Description = request.Description
            };

            validator.TestValidate(failRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveAnyValidationErrorWhenRequestHaveIntersectionWithTheEndTime()
        {
            var failRequest = new WorkTime
            {
                UserId = request.UserId,
                StartDate = request.StartDate.AddHours(1),
                EndDate = request.EndDate.AddHours(1),
                Title = request.Title,
                ProjectId = request.ProjectId,
                Description = request.Description
            };

            validator.TestValidate(failRequest).ShouldHaveAnyValidationError();
        }
    }
}