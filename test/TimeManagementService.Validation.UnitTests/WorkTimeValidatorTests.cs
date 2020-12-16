using FluentValidation;
using FluentValidation.TestHelper;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace LT.DigitalOffice.TimeManagementService.Validation.UnitTests
{
    public class WorkTimeValidatorTests
    {
        private Mock<IAssignUserValidator> mockUserValidator;
        private Mock<IAssignProjectValidator> mockProjectValidator;
        private IValidator<WorkTime> validator;
        private WorkTime request;

        [SetUp]
        public void SetUp()
        {
            request = new WorkTime
            {
                Id = null,
                UserId = Guid.NewGuid(),
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now,
                Title = "Example",
                Description = "ExampleDescription",
                ProjectId = Guid.NewGuid(),
                CurrentUserId = Guid.NewGuid(),
                Minutes = 5
            };

            mockUserValidator = new Mock<IAssignUserValidator>();
            mockProjectValidator = new Mock<IAssignProjectValidator>();

            mockUserValidator
                .Setup(x => x.CanAssignUser(request.CurrentUserId, (Guid)request.UserId))
                .Returns(true);

            mockUserValidator
                .Setup(x => x.CanAssignUser(request.CurrentUserId, request.CurrentUserId))
                .Returns(true);

            mockProjectValidator
                .Setup(x => x.CanAssignProject((Guid)request.UserId, request.ProjectId))
                .Returns(true);

            mockProjectValidator
                .Setup(x => x.CanAssignProject(request.CurrentUserId, request.ProjectId))
                .Returns(true);

            validator = new WorkTimeValidator(mockUserValidator.Object, mockProjectValidator.Object);
        }

        [Test]
        public void ShouldNotHaveValidationErrorWhenRequestIsValid1()
        {
            request.UserId = null;

            validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void ShouldNotHaveValidationErrorWhenRequestIsValid2()
        {
            validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void ShouldHaveValidationErrorWhenAssignUserValidatorReturnFalse1()
        {
            mockProjectValidator
                .Setup(x => x.CanAssignProject((Guid)request.UserId, request.ProjectId))
                .Returns(false);

            validator.TestValidate(request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveValidationErrorWhenAssignUserValidatorReturnFalse2()
        {
            request.UserId = null;
            mockProjectValidator
                .Setup(x => x.CanAssignProject(request.CurrentUserId, request.ProjectId))
                .Returns(false);

            validator.TestValidate(request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveValidationErrorWhenAssignProjectValidatorReturnFalse1()
        {
            mockUserValidator
                .Setup(x => x.CanAssignUser(request.CurrentUserId, (Guid)request.UserId))
                .Returns(false);

            validator.TestValidate(request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveValidationErrorWhenAssignProjectValidatorReturnFalse2()
        {
            request.UserId = null;
            mockUserValidator
                .Setup(x => x.CanAssignUser(request.CurrentUserId, request.CurrentUserId))
                .Returns(false);

            validator.TestValidate(request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveValidationErrorWhenUserIdIsEmpty()
        {
            request.UserId = Guid.Empty;

            validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenStartDateIsEmpty()
        {
            request.StartDate = new DateTime();

            validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.StartDate);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenEndDateIsEmpty()
        {
            request.EndDate = new DateTime();

            validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.EndDate);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenMinutesLessThat1()
        {
            request.Minutes = 0;

            validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Minutes);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenTitleIsTooLong()
        {
            request.Title = "".PadLeft(129);

            validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenDescriptionIsTooLong()
        {
            request.Description = "".PadLeft(1001);

            validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenProjectIdIsEmpty()
        {
            request.ProjectId = Guid.Empty;

            validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.ProjectId);
        }
    }
}
