using FluentValidation;
using FluentValidation.TestHelper;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Enums;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces.Helpers;
using Moq;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Validation.UnitTests
{
    public class LeaveTimeRequestValidatorTests
    {
        private Mock<IUserAssignmentValidator> mockUserValidator;
        private IValidator<LeaveTimeRequest> validator;
        private LeaveTimeRequest request;

        [SetUp]
        public void SetUp()
        {
            request = new LeaveTimeRequest
            {
                UserId = Guid.NewGuid(),
                StartTime = DateTime.Now.AddDays(-1),
                EndTime = DateTime.Now,
                Comment = "ExampleComment",
                LeaveType = LeaveType.Vacation,
                CurrentUserId = Guid.NewGuid()
            };

            mockUserValidator = new Mock<IUserAssignmentValidator>();

            mockUserValidator
                .Setup(x => x.UserCanAssignUser(request.CurrentUserId, (Guid)request.UserId))
                .Returns(true);

            mockUserValidator
                .Setup(x => x.UserCanAssignUser(request.CurrentUserId, request.CurrentUserId))
                .Returns(true);

            validator = new LeaveTimeRequestValidator(mockUserValidator.Object);
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
            mockUserValidator
                .Setup(x => x.UserCanAssignUser(request.CurrentUserId, (Guid)request.UserId))
                .Returns(false);

            validator.TestValidate(request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveValidationErrorWhenAssignUserValidatorReturnFalse2()
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

            request.UserId = Guid.Empty;

            validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenStartTimeIsEmpty()
        {
            request.StartTime = new DateTime();

            validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.StartTime);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenEndDateIsEmpty()
        {
            request.EndTime = new DateTime();

            validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.EndTime);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenProjectIdIsEmpty()
        {
            request.LeaveType = (LeaveType)100;

            validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.LeaveType);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenCommentIsTooLong()
        {
            request.Comment = "".PadLeft(10001);

            validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Comment);
        }
    }
}
