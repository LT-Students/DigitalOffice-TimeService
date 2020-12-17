using FluentValidation;
using FluentValidation.TestHelper;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Enums;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Moq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeManagementService.Validation.UnitTests
{
    class EditLeaveTimeRequestValidatorTests
    {
        private Mock<ILeaveTimeRepository> mockRepository;
        private Mock<IAssignUserValidator> mockUserValidator;
        private IValidator<EditLeaveTimeRequest> validator;
        private EditLeaveTimeRequest editRequest;
        private IContractResolver resolver;

        private Guid currentUserId = Guid.NewGuid();
        private Guid assignedUserId = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            resolver = new CamelCasePropertyNamesContractResolver();

            editRequest = new EditLeaveTimeRequest
            {
                Patch = new JsonPatchDocument<DbLeaveTime>(new List<Operation<DbLeaveTime>>
                {
                    new Operation<DbLeaveTime>("replace", EditLeaveTimeRequestValidator.UserIdPath, "", assignedUserId),
                    new Operation<DbLeaveTime>("replace", EditLeaveTimeRequestValidator.StartTimePath, "", DateTime.Now.AddDays(-1)),
                    new Operation<DbLeaveTime>("replace", EditLeaveTimeRequestValidator.EndTimePath, "", DateTime.Now),
                    new Operation<DbLeaveTime>("replace", EditLeaveTimeRequestValidator.LeaveTypePath, "", LeaveType.SickLeave),
                    new Operation<DbLeaveTime>("replace", EditLeaveTimeRequestValidator.CommentPath, "", "Example comment")
                }, resolver),
                LeaveTimeId = Guid.NewGuid(),
                CurrentUserId = currentUserId
            };

            mockRepository = new Mock<ILeaveTimeRepository>();
            mockUserValidator = new Mock<IAssignUserValidator>();

            mockUserValidator
                .Setup(x => x.CanAssignUser(currentUserId, assignedUserId))
                .Returns(true);

            mockUserValidator
                .Setup(x => x.CanAssignUser(currentUserId, currentUserId))
                .Returns(true);

            validator = new EditLeaveTimeRequestValidator(mockRepository.Object, mockUserValidator.Object);
        }

        Func<string, Operation> GetOperationByPath =>
            (path) => editRequest.Patch.Operations.Find(x => x.path == path);

        [Test]
        public void ShouldValidateEditProjectRequestWhenRequestIsCorrect()
        {
            validator.TestValidate(editRequest).ShouldNotHaveAnyValidationErrors();
        }

        #region Base validation
        [Test]
        public void ShouldThrowValidationExceptionWhenRequestNotContainsOperations()
        {
            editRequest.Patch.Operations.Clear();

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldThrowValidationExceptionWhenRequestContainsNotUniqueOperations()
        {
            editRequest.Patch.Operations.Add(editRequest.Patch.Operations.First());

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldThrowValidationExceptionWhenRequestContainsNotSupportedReplace()
        {
            editRequest.Patch.Operations.Add(new Operation<DbLeaveTime>("replace", "/Id", "", Guid.NewGuid().ToString()));

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }
        #endregion

        #region field validations
        [Test]
        public void ShouldHaveValidationErrorWhenStartTimeAfterEndTime()
        {
            var temp = GetOperationByPath(EditLeaveTimeRequestValidator.StartTimePath).value;

            GetOperationByPath(EditLeaveTimeRequestValidator.StartTimePath).value =
                GetOperationByPath(EditLeaveTimeRequestValidator.EndTimePath).value;

            GetOperationByPath(EditLeaveTimeRequestValidator.EndTimePath).value = temp;

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        // test assignedUserId

        [Test]
        public void ShouldValidateEditProjectRequestWhenUserIdIsValid()
        {
            SuccessTestsWithOperationsForPath(EditLeaveTimeRequestValidator.UserIdPath, false);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenUserIdIsEmpty()
        {
            GetOperationByPath(EditLeaveTimeRequestValidator.UserIdPath).value = Guid.Empty;

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldValidateEditProjectRequestWhenLeaveTypeIsValid()
        {
            SuccessTestsWithOperationsForPath(EditLeaveTimeRequestValidator.LeaveTypePath, false);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenLeaveTypeIsNotCorrect()
        {
            GetOperationByPath(EditLeaveTimeRequestValidator.LeaveTypePath).value = (LeaveType)100;

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldValidateEditProjectRequestWhenStartTimeIsValid()
        {
            SuccessTestsWithOperationsForPath(EditLeaveTimeRequestValidator.StartTimePath, false);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenStartTimeIsEmpty()
        {
            GetOperationByPath(EditLeaveTimeRequestValidator.StartTimePath).value = new DateTime();

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldValidateEditProjectRequestWhenEndTimeIsValid()
        {
            SuccessTestsWithOperationsForPath(EditLeaveTimeRequestValidator.EndTimePath, false);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenEndTimeIsEmpty()
        {
            GetOperationByPath(EditLeaveTimeRequestValidator.EndTimePath).value = new DateTime();

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldValidateEditProjectRequestWhenCommentIsValid()
        {
            SuccessTestsWithOperationsForPath(EditLeaveTimeRequestValidator.CommentPath, true);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenCommentIsTooSmall()
        {
            GetOperationByPath(EditLeaveTimeRequestValidator.CommentPath).value = "".PadLeft(501);

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        public void SuccessTestsWithOperationsForPath(string path, bool nullable)
        {
            GetOperationByPath(path).op = "add";
            validator.TestValidate(editRequest).ShouldNotHaveAnyValidationErrors();

            GetOperationByPath(path).op = "replace";
            validator.TestValidate(editRequest).ShouldNotHaveAnyValidationErrors();

            if (nullable)
            {
                GetOperationByPath(path).op = "remove";
                validator.TestValidate(editRequest).ShouldNotHaveAnyValidationErrors();
            }
        }
        #endregion
    }
}
