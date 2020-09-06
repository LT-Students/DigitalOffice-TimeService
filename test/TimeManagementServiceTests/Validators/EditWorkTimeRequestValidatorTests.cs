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
    class EditWorkTimeRequestValidatorTests
    {
        private IValidator<EditWorkTimeRequest> validator;

        private Mock<IWorkTimeRepository> repositoryMock;

        [SetUp]
        public void SetUp()
        {
            repositoryMock = new Mock<IWorkTimeRepository>();

            validator = new EditWorkTimeRequestValidator(repositoryMock.Object);
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenIdIsEmpty()
        {
            validator.ShouldHaveValidationErrorFor(x => x.Id, Guid.Empty);
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenWorkerUserIdIsEmpty()
        {
            validator.ShouldHaveValidationErrorFor(x => x.WorkerUserId, Guid.Empty);
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenStartTimeIsEmpty()
        {
            validator.ShouldHaveValidationErrorFor(x => x.StartTime, new DateTime());
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenEndTimeIsEmpty()
        {
            validator.ShouldHaveValidationErrorFor(x => x.EndTime, new DateTime());
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenTitleIsEmpty()
        {
            validator.ShouldHaveValidationErrorFor(x => x.Title, "");
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenTitleIsTooLong()
        {
            var title = "123" + new string('a', 30);

            validator.ShouldHaveValidationErrorFor(x => x.Title, title);
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenTitleIsTooShort()
        {
            validator.ShouldHaveValidationErrorFor(x => x.Title, "A");
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenProjectIdIsEmpty()
        {
            validator.ShouldHaveValidationErrorFor(x => x.ProjectId, Guid.Empty);
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenDescriptionIsTooLong()
        {
            var comment = "1" + new string('a', 500);

            validator.ShouldHaveValidationErrorFor(x => x.Description, comment);
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenEndTimeIsLessThanStartTime()
        {
            var request = new EditWorkTimeRequest
            {
                Id = Guid.NewGuid(),
                WorkerUserId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 2, 2, 2, 2, 2),
                EndTime = new DateTime(2020, 1, 1, 1, 1, 1),
                Title = "ExampleTitle",
                ProjectId = Guid.NewGuid(),
                Description = "ExampleDescription"
            };

            Assert.Throws<ValidationException>(() => validator.ValidateAndThrow(request));
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenWorkTimeGreaterThanWorkingLimit()
        {
            var request = new EditWorkTimeRequest
            {
                Id = Guid.NewGuid(),
                WorkerUserId = Guid.NewGuid(),
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(25),
                Title = "ExampleTitle",
                ProjectId = Guid.NewGuid(),
                Description = "ExampleDescription"
            };

            Assert.Throws<ValidationException>(() => validator.ValidateAndThrow(request));
        }

        [Test]
        public void ShouldHaveAnyValidationErrorWhenRequestHaveIntersectionWithTheStartTime()
        {
            var request = new EditWorkTimeRequest
            {
                Id = Guid.NewGuid(),
                WorkerUserId = Guid.NewGuid(),
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(25),
                Title = "ExampleTitle",
                ProjectId = Guid.NewGuid(),
                Description = "ExampleDescription"
            };

            var time = new DbWorkTime
            {
                WorkerUserId = request.WorkerUserId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Title = request.Title,
                ProjectId = request.ProjectId,
                Description = request.Description
            };

            repositoryMock.Setup(x => x.GetUserWorkTimes(request.WorkerUserId, It.IsAny<WorkTimeFilter>()))
                .Returns(new List<DbWorkTime> { time });

            Assert.Throws<ValidationException>(() => validator.ValidateAndThrow(request));
        }

        [Test]
        public void ShouldNotThrowValidationExceptionWhenDataIsValid()
        {
            var request = new EditWorkTimeRequest
            {
                Id = Guid.NewGuid(),
                WorkerUserId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 1, 1, 1, 1, 1),
                EndTime = new DateTime(2020, 1, 1, 2, 2, 2),
                Title = "ExampleTitle",
                ProjectId = Guid.NewGuid(),
                Description = "ExampleDescription"
            };

            Assert.DoesNotThrow(() => validator.ValidateAndThrow(request));
        }
    }
}