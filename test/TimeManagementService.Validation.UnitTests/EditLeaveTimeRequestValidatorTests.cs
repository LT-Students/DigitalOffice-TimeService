using FluentValidation;
using FluentValidation.TestHelper;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
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

        [SetUp]
        public void SetUp()
        {
            resolver = new CamelCasePropertyNamesContractResolver();

            editRequest = new EditLeaveTimeRequest
            {
                Patch = new JsonPatchDocument<DbLeaveTime>(new List<Operation<DbLeaveTime>>
                {
                    //new Operation<DbWorkTime>("replace", "/Description", "", "Example description"),
                }, resolver),
                LeaveTimeId = Guid.NewGuid()
            };

            mockRepository = new Mock<ILeaveTimeRepository>();
            mockUserValidator = new Mock<IAssignUserValidator>();

            //mockUserValidator
            //    .Setup(x => x.CanAssignUser(editRequest.CurrentUserId, (Guid)editRequest.UserId))
            //    .Returns(true);

            //mockUserValidator
            //    .Setup(x => x.CanAssignUser(editRequest.CurrentUserId, editRequest.CurrentUserId))
            //    .Returns(true);

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
        // ADD TESTS

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
