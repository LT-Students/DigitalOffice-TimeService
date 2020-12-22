using FluentValidation;
using FluentValidation.TestHelper;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces.Helpers;
using Moq;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Validation.UnitTests
{
    public class WorkTimeRequestValidatorTests
    {
        private Mock<IUserAssignmentValidator> mockUserValidator;
        private Mock<IProjectAssignmentValidator> mockProjectValidator;
        private IValidator<WorkTimeRequest> validator;
        private WorkTimeRequest request;

        [SetUp]
        public void SetUp()
        {
            request = new WorkTimeRequest
            {
                UserId = Guid.NewGuid(),
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now,
                Title = "Example",
                Description = "ExampleDescription",
                ProjectId = Guid.NewGuid(),
                CurrentUserId = Guid.NewGuid(),
                Minutes = 5
            };

            mockUserValidator = new Mock<IUserAssignmentValidator>();
            mockProjectValidator = new Mock<IProjectAssignmentValidator>();

            mockUserValidator
                .Setup(x => x.UserCanAssignUser(request.CurrentUserId, (Guid)request.UserId))
                .Returns(true);

            mockUserValidator
                .Setup(x => x.UserCanAssignUser(request.CurrentUserId, request.CurrentUserId))
                .Returns(true);

            mockProjectValidator
                .Setup(x => x.CanAssignUserToProject((Guid)request.UserId, request.ProjectId))
                .Returns(true);

            mockProjectValidator
                .Setup(x => x.CanAssignUserToProject(request.CurrentUserId, request.ProjectId))
                .Returns(true);

            validator = new WorkTimeRequestValidator(mockUserValidator.Object, mockProjectValidator.Object);
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
                .Setup(x => x.CanAssignUserToProject((Guid)request.UserId, request.ProjectId))
                .Returns(false);

            validator.TestValidate(request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveValidationErrorWhenAssignUserValidatorReturnFalse2()
        {
            request.UserId = null;
            mockProjectValidator
                .Setup(x => x.CanAssignUserToProject(request.CurrentUserId, request.ProjectId))
                .Returns(false);

            validator.TestValidate(request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveValidationErrorWhenAssignProjectValidatorReturnFalse1()
        {
            mockUserValidator
                .Setup(x => x.UserCanAssignUser(request.CurrentUserId, (Guid)request.UserId))
                .Returns(false);

            validator.TestValidate(request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveValidationErrorWhenAssignProjectValidatorReturnFalse2()
        {
            request.UserId = null;
            mockUserValidator
                .Setup(x => x.UserCanAssignUser(request.CurrentUserId, request.CurrentUserId))
                .Returns(false);

            validator.TestValidate(request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveValidationErrorWhenUserIdIsEmpty()
        {
            mockUserValidator
                .Setup(x => x.UserCanAssignUser(request.CurrentUserId, Guid.Empty))
                .Returns(true);

            mockProjectValidator
                .Setup(x => x.CanAssignUserToProject(Guid.Empty, request.ProjectId))
                .Returns(true);

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
        public void ShouldHaveValidationErrorWhenTitleIsTooSmall()
        {
            request.Title = "";
            validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Title);

            request.Title = "a";
            validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Title);
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
