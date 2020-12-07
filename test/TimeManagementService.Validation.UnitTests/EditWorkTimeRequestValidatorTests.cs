using FluentValidation;
using FluentValidation.TestHelper;
using LT.DigitalOffice.TimeManagementService.Data.Filters;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
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
        private IValidator<EditWorkTimeRequest> validator;
        private Mock<IWorkTimeRepository> mockRepository;
        private EditWorkTimeRequest editRequest;
        private DbWorkTime variableWorkTime;
        private DbWorkTime oldWorkTime;


        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            variableWorkTime = new DbWorkTime
            {
                Id = Guid.NewGuid(),
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(6),
                WorkerUserId = Guid.NewGuid()
            };

            oldWorkTime = new DbWorkTime
            {
                Id = Guid.NewGuid(),
                StartTime = DateTime.Now.AddDays(-1),
                EndTime = DateTime.Now.AddDays(-1).AddHours(6),
                WorkerUserId = variableWorkTime.WorkerUserId
            };

            mockRepository = new Mock<IWorkTimeRepository>();

            mockRepository
                .Setup(x => x.GetWorkTime(variableWorkTime.Id))
                .Returns(variableWorkTime);

            mockRepository
                .Setup(x => x.GetUserWorkTimes(variableWorkTime.WorkerUserId, It.IsAny<WorkTimeFilter>()))
                .Returns(new List<DbWorkTime> { variableWorkTime, oldWorkTime });

            validator = new EditWorkTimeRequestValidator(mockRepository.Object);
        }

        [SetUp]
        public void SetUp()
        {
            editRequest = new EditWorkTimeRequest
            {
                Patch = new JsonPatchDocument<DbWorkTime>(new List<Operation<DbWorkTime>>
                {
                    new Operation<DbWorkTime>("replace", "/Title", "", "New Name"),
                    new Operation<DbWorkTime>("replace", "/Description", "", "New description"),
                    new Operation<DbWorkTime>("replace", "/ProjectId", "", Guid.NewGuid()),
                    //new Operation<DbWorkTime>("replace", "/WorkerUserId", "", Guid.NewGuid()),
                }, new CamelCasePropertyNamesContractResolver()),
                WorkTimeId = variableWorkTime.Id
            };
        }

        [Test]
        public void ShouldValidateEditProjectRequestWhenRequestIsCorrect()
        {
            validator.TestValidate(editRequest).ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void ShouldValidateEditProjectRequestWhenRequestIsCorrect2()
        {
            editRequest.Patch.Operations.Add(new Operation<DbWorkTime>("replace", "/StartTime", "", variableWorkTime.StartTime.AddHours(1)));

            validator.TestValidate(editRequest).ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void ShouldValidateEditProjectRequestWhenRequestIsCorrect3()
        {
            editRequest.Patch.Operations.Add(new Operation<DbWorkTime>("replace", "/EndTime", "", variableWorkTime.EndTime.AddHours(1)));

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
        public void ShouldThrowValidationExceptionWhenShortNameIsTooLong()
        {
            editRequest.Patch.Operations.Find(x => x.path == "/Title").value = "".PadLeft(33);

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldThrowValidationExceptionWhenShortNameIsTooShort()
        {
            editRequest.Patch.Operations.Find(x => x.path == "/Title").value = "A";

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldThrowValidationExceptionWhenDescriptionIsTooLong()
        {
            editRequest.Patch.Operations.Find(x => x.path == "/Description").value = "".PadLeft(501);

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenStartTimeIsEmpty()
        {
            editRequest.Patch.Operations.Add(new Operation<DbWorkTime>("replace", "/StartTime", "", new DateTime()));

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenEndTimeIsEmpty()
        {
            editRequest.Patch.Operations.Add(new Operation<DbWorkTime>("replace", "/EndTime", "", new DateTime()));

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }


        [Test]
        public void ShouldThrowsValidationExceptionWhenProjectIdIsEmpty()
        {
            editRequest.Patch.Operations.Find(x => x.path == "/ProjectId").value = Guid.Empty;

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        //[Test]
        //public void ShouldThrowsValidationExceptionWhenWorkerUserIdIsEmpty()
        //{
        //    editRequest.Patch.Operations.Find(x => x.path == "/WorkerUserId").value = Guid.Empty;

        //    validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        //}

        [Test]
        public void ShouldThrowsValidationExceptionWhenEndTimeIsLessThanStartTime()
        {
            editRequest.Patch.Operations.Add(new Operation<DbWorkTime>("replace", "/StartTime", "", variableWorkTime.EndTime));
            editRequest.Patch.Operations.Add(new Operation<DbWorkTime>("replace", "/EndTime", "", variableWorkTime.StartTime));

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenWorkTimeGreaterThanWorkingLimit()
        {
            editRequest.Patch.Operations.Add(new Operation<DbWorkTime>("replace", "/StartTime", "", DateTime.Now));
            editRequest.Patch.Operations.Add(new Operation<DbWorkTime>("replace", "/EndTime", "", DateTime.Now.Add(EditWorkTimeRequestValidator.WorkingLimit).AddHours(1)));

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenNewStartTimeIsLessThanOldStartTimeAndNewEndTimeLessThanOldEndTime()
        {
            editRequest.Patch.Operations.Add(new Operation<DbWorkTime>("replace", "/StartTime", "", oldWorkTime.StartTime.AddHours(-1)));
            editRequest.Patch.Operations.Add(new Operation<DbWorkTime>("replace", "/EndTime", "", oldWorkTime.EndTime.AddHours(-1)));

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenNewStartTimeIsLessThanOldStartTimeAndNewEndTimeGreatherThanOldEndTime()
        {
            editRequest.Patch.Operations.Add(new Operation<DbWorkTime>("replace", "/StartTime", "", oldWorkTime.StartTime.AddHours(-1)));
            editRequest.Patch.Operations.Add(new Operation<DbWorkTime>("replace", "/EndTime", "", oldWorkTime.EndTime.AddHours(1)));

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenNewStartTimeIsGreatherThanOldStartTimeAndNewEndTimeLessThanOldEndTime()
        {
            editRequest.Patch.Operations.Add(new Operation<DbWorkTime>("replace", "/StartTime", "", oldWorkTime.StartTime.AddHours(1)));
            editRequest.Patch.Operations.Add(new Operation<DbWorkTime>("replace", "/EndTime", "", oldWorkTime.EndTime.AddHours(-1)));

            validator.TestValidate(editRequest).ShouldHaveAnyValidationError();
        }
        #endregion
    }
}