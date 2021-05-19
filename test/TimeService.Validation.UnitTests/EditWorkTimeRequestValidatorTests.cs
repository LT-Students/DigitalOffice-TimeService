using FluentValidation;
using FluentValidation.TestHelper;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Models.Dto.Requests.HelpersModels;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Moq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Validation.UnitTests
{
    class EditWorkTimeRequestValidatorTests
    {
        private IValidator<EditWorkTimeModel> _validator;

        private Mock<IWorkTimeRepository> _repositoryMock;

        private EditWorkTimeModel _request;

        Func<string, Operation> GetOperationByPath =>
            (path) => _request.JsonPatchDocument.Operations.Find(x => x.path == path);

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _repositoryMock = new Mock<IWorkTimeRepository>();

            _validator = new EditWorkTimeRequestValidator(_repositoryMock.Object);
        }

        [SetUp]
        public void SetUp()
        {
            _request = new EditWorkTimeModel
            {
                JsonPatchDocument = new JsonPatchDocument<EditWorkTimeRequest>(new List<Operation<EditWorkTimeRequest>>
                {
                    new Operation<EditWorkTimeRequest>(
                        "replace",
                        $"/{nameof(EditWorkTimeRequest.Title)}",
                        "",
                        "new title"),

                    new Operation<EditWorkTimeRequest>(
                        "replace",
                        $"/{nameof(EditWorkTimeRequest.Description)}",
                        "",
                        "new description"),

                    new Operation<EditWorkTimeRequest>(
                        "replace",
                        $"/{nameof(EditWorkTimeRequest.ProjectId)}",
                        "",
                        Guid.NewGuid()),

                    new Operation<EditWorkTimeRequest>(
                        "replace",
                        $"/{nameof(EditWorkTimeRequest.StartTime)}",
                        "",
                        DateTime.Now.AddHours(-3)),

                    new Operation<EditWorkTimeRequest>(
                        "replace",
                        $"/{nameof(EditWorkTimeRequest.EndTime)}",
                        "",
                        DateTime.Now.AddHours(-1))
                }, new CamelCasePropertyNamesContractResolver()),
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };
        }

        [Test]
        public void SuccessValidation()
        {
            _validator.TestValidate(_request).ShouldNotHaveAnyValidationErrors();
        }

        #region Base validate errors

        [Test]
        public void ExceptionWhenRequestNotContainsOperations()
        {
            _request.JsonPatchDocument.Operations.Clear();

            _validator.TestValidate(_request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ExceptionWhenRequestContainsNotUniqueOperations()
        {
            _request.JsonPatchDocument.Operations.Add(_request.JsonPatchDocument.Operations.First());

            _validator.TestValidate(_request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ExceptionWhenRequestContainsNotSupportedReplace()
        {
            _request.JsonPatchDocument.Operations.Add(new Operation<EditWorkTimeRequest>("replace", $"/{nameof(DbWorkTime.Id)}", "", Guid.NewGuid()));

            _validator.TestValidate(_request).ShouldHaveAnyValidationError();
        }
        #endregion

        #region WorkTime fields checks

        [Test]
        public void ExceptionWhenTitkeIsTooLong()
        {
            GetOperationByPath(EditWorkTimeRequestValidator.Title).value = "".PadLeft(201);

            _validator.TestValidate(_request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ExceptionWhenTitkeIsEmpty()
        {
            GetOperationByPath(EditWorkTimeRequestValidator.Title).value = "";

            _validator.TestValidate(_request).ShouldHaveAnyValidationError();
        }

        //[Test]
        //public void ExceptionWhenProjectIdIsEmpty()
        //{
        //    GetOperationByPath(EditWorkTimeRequestValidator.ProjectId).value = Guid.Empty;

        //    _validator.TestValidate(_request).ShouldHaveAnyValidationError();
        //}

        [Test]
        public void ShouldThrowsValidationExceptionWhenStartTimeIsEmpty()
        {
            GetOperationByPath(EditWorkTimeRequestValidator.StartTime).value = new DateTime();

            _validator.TestValidate(_request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenEndTimeIsEmpty()
        {
            GetOperationByPath(EditWorkTimeRequestValidator.EndTime).value = new DateTime();

            _validator.TestValidate(_request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenEndTimeIsLessThanStartTime()
        {
            GetOperationByPath(EditWorkTimeRequestValidator.EndTime).value =
                ((DateTime)GetOperationByPath(EditWorkTimeRequestValidator.StartTime).value).AddHours(-1);

            _validator.TestValidate(_request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldThrowsValidationExceptionWhenWorkTimeGreaterThanWorkingLimit()
        {
            GetOperationByPath(EditWorkTimeRequestValidator.EndTime).value = 
                ((DateTime)GetOperationByPath(EditWorkTimeRequestValidator.EndTime).value).Add(EditWorkTimeRequestValidator.WorkingLimit);

            _validator.TestValidate(_request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveAnyValidationErrorWhenRequestHaveIntersectionWithTheStartTime()
        {
            var time = new DbWorkTime
            {
                Id = Guid.NewGuid(),
                StartTime = ((DateTime)GetOperationByPath(EditWorkTimeRequestValidator.StartTime).value).AddHours(-1),
                EndTime = ((DateTime)GetOperationByPath(EditWorkTimeRequestValidator.StartTime).value).AddHours(1)
            };

            _repositoryMock.Setup(x => x.GetUserWorkTimes(_request.UserId, It.IsAny<WorkTimeFilter>()))
                .Returns(new List<DbWorkTime> { time });

            _validator.TestValidate(_request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveAnyValidationErrorWhenRequestHaveIntersectionWithTheEndTime()
        {
            var time = new DbWorkTime
            {
                Id = Guid.NewGuid(),
                StartTime = ((DateTime)GetOperationByPath(EditWorkTimeRequestValidator.EndTime).value).AddHours(-1),
                EndTime = ((DateTime)GetOperationByPath(EditWorkTimeRequestValidator.EndTime).value).AddHours(1)
            };

            _repositoryMock.Setup(x => x.GetUserWorkTimes(_request.UserId, It.IsAny<WorkTimeFilter>()))
                .Returns(new List<DbWorkTime> { time });

            _validator.TestValidate(_request).ShouldHaveAnyValidationError();
        }

        [Test]
        public void ShouldHaveAnyValidationErrorWhenRequestTimeBetweenOtherStartTimeAndEndTime()
        {
            var time = new DbWorkTime
            {
                Id = Guid.NewGuid(),
                StartTime = ((DateTime)GetOperationByPath(EditWorkTimeRequestValidator.StartTime).value).AddMinutes(1),
                EndTime = ((DateTime)GetOperationByPath(EditWorkTimeRequestValidator.EndTime).value).AddMinutes(-1)
            };

            _repositoryMock.Setup(x => x.GetUserWorkTimes(_request.UserId, It.IsAny<WorkTimeFilter>()))
                .Returns(new List<DbWorkTime> { time });

            _validator.TestValidate(_request).ShouldHaveAnyValidationError();
        }

        #endregion
    }
}