using FluentValidation;
using FluentValidation.TestHelper;
using LT.DigitalOffice.TimeManagementService.Database.Entities;
using LT.DigitalOffice.TimeManagementService.Models;
using LT.DigitalOffice.TimeManagementService.Repositories.Filters;
using LT.DigitalOffice.TimeManagementService.Repositories.Interfaces;
using LT.DigitalOffice.TimeManagementService.Validators;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeManagementServiceUnitTests.Validators
{
    public class CreateWorkTimeRequestValidatorTests
    {
        private Mock<IWorkTimeRepository> repositoryMock;
        private IValidator<CreateWorkTimeRequest> validator;

        private CreateWorkTimeRequest request;
        private DbWorkTime expectedDbWorkTime;

        [SetUp]
        public void Setup()
        {
            repositoryMock = new Mock<IWorkTimeRepository>();

            validator = new CreateWorkTimeRequestValidator(repositoryMock.Object);

            request = new CreateWorkTimeRequest
            {
                WorkerUserId = Guid.NewGuid(),
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(6),
                Title = "Worked...",
                ProjectId = Guid.NewGuid(),
                Description = "Did something"
            };

            expectedDbWorkTime = new DbWorkTime
            {
                WorkerUserId = request.WorkerUserId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Title = request.Title,
                ProjectId = request.ProjectId,
                Description = request.Description
            };

            repositoryMock.Setup(x => x.GetUserWorkTimes(request.WorkerUserId, It.IsAny<WorkTimeFilter>()))
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
            request.WorkerUserId = Guid.Empty;
            repositoryMock.Setup(x => x.GetUserWorkTimes(It.IsAny<Guid>(), It.IsAny<WorkTimeFilter>()))
                .Returns(new List<DbWorkTime>());

            validator.TestValidate(request).ShouldHaveValidationErrorFor(r => r.WorkerUserId);
        }
        #endregion

        #region StartTime
        [Test]
        public void ShouldHaveValidationErrorWhenStartTimeTooEarly()
        {
            var startTime = DateTime.Now.AddDays(CreateWorkTimeRequestValidator.ToDay).AddHours(1);
            repositoryMock.Setup(x => x.GetUserWorkTimes(It.IsAny<Guid>(), It.IsAny<WorkTimeFilter>()))
                .Returns(new List<DbWorkTime>());

            validator.ShouldHaveValidationErrorFor(x => x.StartTime, startTime);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenStartTimeTooLate()
        {
            var startTime = DateTime.Now.AddDays(CreateWorkTimeRequestValidator.FromDay).AddHours(-1);
            repositoryMock.Setup(x => x.GetUserWorkTimes(It.IsAny<Guid>(), It.IsAny<WorkTimeFilter>()))
                .Returns(new List<DbWorkTime>());

            validator.ShouldHaveValidationErrorFor(x => x.StartTime, startTime);
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
            var temp = request.StartTime;
            request.StartTime = request.EndTime;
            request.EndTime = temp;

            validator.TestValidate(request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveAnyValidationErrorWhenWorkTimeGreaterThanWorkingLimit()
        {
            var tooManyMinutes = CreateWorkTimeRequestValidator.WorkingLimit.TotalMinutes + 1;
            request.EndTime = request.StartTime.AddMinutes(tooManyMinutes);

            validator.TestValidate(request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldNotHaveAnyValidationErrorsWhenRequestOverlapsWithOtherTime()
        {

            var successfulRequest = new CreateWorkTimeRequest
            {
                WorkerUserId = request.WorkerUserId,
                StartTime = request.StartTime.AddHours(-6),
                EndTime = request.StartTime.AddHours(-5.85),
                Title = request.Title,
                ProjectId = request.ProjectId,
                Description = request.Description
            };

            validator.TestValidate(successfulRequest).ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void ShouldHaveAnyValidationErrorWhenRequestHaveIntersectionWithTheStartTime()
        {
            var failRequest = new CreateWorkTimeRequest
            {
                WorkerUserId = request.WorkerUserId,
                StartTime = request.StartTime.AddHours(-1),
                EndTime = request.EndTime.AddHours(-1),
                Title = request.Title,
                ProjectId = request.ProjectId,
                Description = request.Description
            };

            validator.TestValidate(failRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveAnyValidationErrorWhenRequestHaveIntersectionInsideTime()
        {
            var failRequest = new CreateWorkTimeRequest
            {
                WorkerUserId = request.WorkerUserId,
                StartTime = request.StartTime.AddHours(1),
                EndTime = request.EndTime.AddHours(-1),
                Title = request.Title,
                ProjectId = request.ProjectId,
                Description = request.Description
            };

            validator.TestValidate(failRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveAnyValidationErrorWhenRequestHaveIntersectionWithTheEndTime()
        {
            var failRequest = new CreateWorkTimeRequest
            {
                WorkerUserId = request.WorkerUserId,
                StartTime = request.StartTime.AddHours(1),
                EndTime = request.EndTime.AddHours(1),
                Title = request.Title,
                ProjectId = request.ProjectId,
                Description = request.Description
            };

            validator.TestValidate(failRequest).ShouldHaveAnyValidationError();
        }
    }
}