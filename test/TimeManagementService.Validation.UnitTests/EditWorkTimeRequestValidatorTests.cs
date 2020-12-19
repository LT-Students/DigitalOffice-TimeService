using FluentValidation;
using FluentValidation.TestHelper;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using LT.DigitalOffice.TimeManagementService.Validation.Interfaces.Helpers;
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
    class EditWorkTimeRequestValidatorTests
    {
        private Mock<IWorkTimeRepository> mockRepository;
        private Mock<IUserAssignmentValidator> mockUserValidator;
        private Mock<IProjectAssignmentValidator> mockProjectValidator;
        private IValidator<EditWorkTimeRequest> validator;
        private EditWorkTimeRequest editRequest;
        private IContractResolver resolver;

        private Guid currentUserId = Guid.NewGuid();
        private Guid assignedUserId = Guid.NewGuid();
        private Guid assignedProjectId = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            resolver = new CamelCasePropertyNamesContractResolver();

            editRequest = new EditWorkTimeRequest
            {
                Patch = new JsonPatchDocument<DbWorkTime>(new List<Operation<DbWorkTime>>
                {
                    new Operation<DbWorkTime>("replace", EditWorkTimeRequestValidator.ProjectIdPath, "", assignedProjectId),
                    new Operation<DbWorkTime>("replace", EditWorkTimeRequestValidator.UserIdPath, "", assignedUserId),
                    new Operation<DbWorkTime>("replace", EditWorkTimeRequestValidator.StartTimePath, "", DateTime.Now.AddDays(-1)),
                    new Operation<DbWorkTime>("replace", EditWorkTimeRequestValidator.EndTimePath, "", DateTime.Now),
                    new Operation<DbWorkTime>("replace", EditWorkTimeRequestValidator.TitlePath, "", "Example title"),
                    new Operation<DbWorkTime>("replace", EditWorkTimeRequestValidator.DescriptionPath, "", "Example description")
                }, resolver),
                WorkTimeId = Guid.NewGuid(),
                CurrentUserId = currentUserId
            };

            mockRepository = new Mock<IWorkTimeRepository>();
            mockUserValidator = new Mock<IUserAssignmentValidator>();
            mockProjectValidator = new Mock<IProjectAssignmentValidator>();

            mockUserValidator
                .Setup(x => x.UserCanAssignUser(currentUserId, assignedUserId))
                .Returns(true);

            mockUserValidator
                .Setup(x => x.UserCanAssignUser(currentUserId, currentUserId))
                .Returns(true);

            mockProjectValidator
                .Setup(x => x.CanAssignUserToProject(assignedUserId, assignedProjectId))
                .Returns(true);

            mockProjectValidator
                .Setup(x => x.CanAssignUserToProject(currentUserId, assignedProjectId))
                .Returns(true);

            validator = new EditWorkTimeRequestValidator(mockRepository.Object, mockUserValidator.Object, mockProjectValidator.Object);
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
            editRequest.Patch.Operations.Add(new Operation<DbWorkTime>("replace", "/Id", "", Guid.NewGuid().ToString()));

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }
        #endregion

        #region field validations
        [Test]
        public void ShouldHaveValidationErrorWhenStartTimeAfterEndTime()
        {
            var temp = GetOperationByPath(EditWorkTimeRequestValidator.StartTimePath).value;

            GetOperationByPath(EditWorkTimeRequestValidator.StartTimePath).value =
                GetOperationByPath(EditWorkTimeRequestValidator.EndTimePath).value;

            GetOperationByPath(EditWorkTimeRequestValidator.EndTimePath).value = temp;

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        // test assignedUserId
        // test assignedProjectId

        [Test]
        public void ShouldValidateEditProjectRequestWhenUserIdIsValid()
        {
            SuccessTestsWithOperationsForPath(EditWorkTimeRequestValidator.UserIdPath, false);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenUserIdIsEmpty()
        {
            GetOperationByPath(EditWorkTimeRequestValidator.UserIdPath).value = Guid.Empty;

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldValidateEditProjectRequestWhenProjectIdIsValid()
        {
            SuccessTestsWithOperationsForPath(EditWorkTimeRequestValidator.ProjectIdPath, false);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenProjectIdIsEmpty()
        {
            GetOperationByPath(EditWorkTimeRequestValidator.ProjectIdPath).value = Guid.Empty;

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldValidateEditProjectRequestWhenStartTimeIsValid()
        {
            SuccessTestsWithOperationsForPath(EditWorkTimeRequestValidator.StartTimePath, false);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenStartTimeIsEmpty()
        {
            GetOperationByPath(EditWorkTimeRequestValidator.StartTimePath).value = new DateTime();

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldValidateEditProjectRequestWhenEndTimeIsValid()
        {
            SuccessTestsWithOperationsForPath(EditWorkTimeRequestValidator.EndTimePath, false);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenEndTimeIsEmpty()
        {
            GetOperationByPath(EditWorkTimeRequestValidator.EndTimePath).value = new DateTime();

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldValidateEditProjectRequestWhenTitleIsValid()
        {
            SuccessTestsWithOperationsForPath(EditWorkTimeRequestValidator.TitlePath, false);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenTitleIsTooSmall()
        {
            GetOperationByPath(EditWorkTimeRequestValidator.TitlePath).value = "a";

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveValidationErrorWhenTitleIsTooLong()
        {
            GetOperationByPath(EditWorkTimeRequestValidator.TitlePath).value = "".PadLeft(33);

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldValidateEditProjectRequestWhenDescriptionIsValid()
        {
            SuccessTestsWithOperationsForPath(EditWorkTimeRequestValidator.DescriptionPath, false);
        }

        [Test]
        public void ShouldHaveValidationErrorWhenDescriptionIsTooLong()
        {
            GetOperationByPath(EditWorkTimeRequestValidator.DescriptionPath).value = "".PadLeft(501);

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
