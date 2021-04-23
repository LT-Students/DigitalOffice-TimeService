using FluentValidation;
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
    class EditWorkTimeRequestValidatorTests
    {
        private IEditWorkTimeRequestValidator _validator;

        private Mock<IWorkTimeRepository> _repositoryMock;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IWorkTimeRepository>();

            _validator = new EditWorkTimeRequestValidator(_repositoryMock.Object);
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenIdIsEmpty()
        {
            _validator.ShouldHaveValidationErrorFor(x => x.Id, Guid.Empty);
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenWorkerUserIdIsEmpty()
        {
            _validator.ShouldHaveValidationErrorFor(x => x.WorkerUserId, Guid.Empty);
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenStartTimeIsEmpty()
        {
            _validator.ShouldHaveValidationErrorFor(x => x.StartTime, new DateTime());
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenEndTimeIsEmpty()
        {
            _validator.ShouldHaveValidationErrorFor(x => x.EndTime, new DateTime());
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenTitleIsEmpty()
        {
            _validator.ShouldHaveValidationErrorFor(x => x.Title, "");
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenTitleIsTooLong()
        {
            var title = "123" + new string('a', 30);

            _validator.ShouldHaveValidationErrorFor(x => x.Title, title);
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenTitleIsTooShort()
        {
            _validator.ShouldHaveValidationErrorFor(x => x.Title, "A");
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenProjectIdIsEmpty()
        {
            _validator.ShouldHaveValidationErrorFor(x => x.ProjectId, Guid.Empty);
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenDescriptionIsTooLong()
        {
            var comment = "1" + new string('a', 500);

            _validator.ShouldHaveValidationErrorFor(x => x.Description, comment);
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

            Assert.Throws<ValidationException>(() => _validator.ValidateAndThrow(request));
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

            Assert.Throws<ValidationException>(() => _validator.ValidateAndThrow(request));
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
                UserId = request.WorkerUserId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Title = request.Title,
                ProjectId = request.ProjectId,
                Description = request.Description
            };

            _repositoryMock.Setup(x => x.GetUserWorkTimes(request.WorkerUserId, It.IsAny<WorkTimeFilter>()))
                .Returns(new List<DbWorkTime> { time });

            Assert.Throws<ValidationException>(() => _validator.ValidateAndThrow(request));
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

            Assert.DoesNotThrow(() => _validator.ValidateAndThrow(request));
        }
    }
}