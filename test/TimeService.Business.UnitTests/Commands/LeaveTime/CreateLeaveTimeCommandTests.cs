using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Helpers;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Business.Helpers.Workdays.Intergations.Interface;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces;
using LT.DigitalOffice.UnitTestKernel;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;

namespace LT.DigitalOffice.TimeService.Business.UnitTests.Commands.LeaveTime
{
  public class CreateLeaveTimeCommandTests
  {
    private AutoMocker _mocker;
    private ICreateLeaveTimeCommand _command;

    private const string Holidays = "1100000110000011000001100000110";
    private const string NullString = null;

    private CreateLeaveTimeRequest _request;
    private CreateLeaveTimeRequest _prolongedRequest;
    private DbLeaveTime _createdLeaveTime;
    private DbWorkTimeMonthLimit _monthLimit;
    private OperationResultResponse<Guid?> _goodResponse;
    private OperationResultResponse<Guid?> _badRequestResponse;
    private OperationResultResponse<Guid?> _forbiddenResponse;
    private OperationResultResponse<Guid?> _badGatewayResponse;
    private Guid _createdBy;
    private ValidationResult _goodValidationResult;
    private ValidationResult _badValidationResult;
    private Dictionary<object, object> _itemsWithOwner;
    private Dictionary<object, object> _itemsWithNotOwner;

    private void Verifiable(
      Times ltAccessValidationHelperTimes,
      Times validatorTimes,
      Times responseCreatorTimes,
      Times leaveTimeRepositoryTimes,
      Times mapperTimes,
      Times httpContextAccessorItemsTimes,
      Times workTimeLimitRepositoryTimes,
      Times calendarTimes)
    {
      _mocker.Verify<ILeaveTimeAccessValidationHelper>(x =>
        x.HasRightsAsync(It.IsAny<Guid>()),
        ltAccessValidationHelperTimes);

      _mocker.Verify<ICreateLeaveTimeRequestValidator>(x =>
          x.ValidateAsync(It.IsAny<CreateLeaveTimeRequest>(), default),
        validatorTimes);

      _mocker.Verify<IResponseCreator>(x =>
        x.CreateFailureResponse<Guid?>(It.IsAny<HttpStatusCode>(), It.IsAny<List<string>>()),
        responseCreatorTimes);

      _mocker.Verify<ILeaveTimeRepository>(x =>
          x.CreateAsync(It.IsAny<DbLeaveTime>()),
        leaveTimeRepositoryTimes);

      _mocker.Verify<IDbLeaveTimeMapper>(x =>
          x.Map(It.IsAny<CreateLeaveTimeRequest>(), It.IsAny<double?>(), It.IsAny<string>()),
        mapperTimes);

      _mocker.Verify<IHttpContextAccessor>(x =>
        x.HttpContext.Items,
        httpContextAccessorItemsTimes);

      _mocker.Verify<IWorkTimeMonthLimitRepository>(x =>
        x.GetAsync(It.IsAny<int>(), It.IsAny<int>()),
        workTimeLimitRepositoryTimes);

      _mocker.Verify<ICalendar>(x =>
        x.GetWorkCalendarByMonthAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()),
        calendarTimes);

      _mocker.Resolvers.Clear();
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      _createdBy = Guid.NewGuid();

      _request = new CreateLeaveTimeRequest
      {
        LeaveType = LeaveType.SickLeave,
        Comment = "I have a sore throat",
        StartTime = new DateTime(2022, 10, 24),
        EndTime = new DateTime(2022, 10, 27),
        Minutes = 1920,
        UserId = _createdBy
      };

      _prolongedRequest = new CreateLeaveTimeRequest
      {
        LeaveType = LeaveType.Prolonged,
        Comment = "Prolonged",
        StartTime = new DateTime(2022, 10, 24),
        EndTime = null,
        UserId = _createdBy
      };

      _createdLeaveTime = new DbLeaveTime
      {
        Id = Guid.NewGuid(),
        CreatedBy = _createdBy,
        LeaveType = (int)_request.LeaveType,
        Comment = _request.Comment,
        StartTime = _request.StartTime.UtcDateTime,
        EndTime = _request.EndTime?.UtcDateTime
          ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).AddMilliseconds(-1),
        UserId = _createdBy
      };

      _monthLimit = new()
      {
        Id = Guid.NewGuid(),
        Month = 10,
        Year = 2022,
        Holidays = Holidays,
        NormHours = 168
      };

      _badRequestResponse = new()
      {
        Body = null,
        Errors = new List<string> { "BadRequest" }
      };

      _forbiddenResponse = new()
      {
        Body = null,
        Errors = new List<string> { "Forbidden" }
      };

      _badGatewayResponse = new()
      {
        Body = null,
        Errors = new List<string>()
      };

      _goodResponse = new()
      {
        Body = _createdLeaveTime.Id,
        Errors = new List<string>()
      };

      _goodValidationResult = new();

      _badValidationResult = new(new List<ValidationFailure> { new("property", "error") });

      _itemsWithOwner = new()
      {
        { "UserId", _createdBy }
      };

      _itemsWithNotOwner = new()
      {
        { "UserId", Guid.NewGuid() }
      };
    }

    [SetUp]
    public void SetUp()
    {
      _mocker = new AutoMocker();
      _command = _mocker.CreateInstance<CreateLeaveTimeCommand>();

      _mocker
        .Setup<ILeaveTimeAccessValidationHelper, Task<bool>>(x =>
          x.HasRightsAsync(It.IsAny<Guid>()))
        .ReturnsAsync(true);

      _mocker
        .Setup<ICreateLeaveTimeRequestValidator, Task<ValidationResult>>(x =>
          x.ValidateAsync(It.IsAny<CreateLeaveTimeRequest>(), default))
        .ReturnsAsync(_goodValidationResult);

      _mocker
        .Setup<IResponseCreator, OperationResultResponse<Guid?>>(x =>
          x.CreateFailureResponse<Guid?>(HttpStatusCode.BadRequest, It.IsAny<List<string>>()))
        .Returns(_badRequestResponse);

      _mocker
        .Setup<IResponseCreator, OperationResultResponse<Guid?>>(x =>
          x.CreateFailureResponse<Guid?>(HttpStatusCode.BadGateway, It.IsAny<List<string>>()))
        .Returns(_badGatewayResponse);

      _mocker
        .Setup<IResponseCreator, OperationResultResponse<Guid?>>(x =>
          x.CreateFailureResponse<Guid?>(HttpStatusCode.Forbidden, It.IsAny<List<string>>()))
        .Returns(_forbiddenResponse);

      _mocker
        .Setup<ILeaveTimeRepository, Task<Guid?>>(x =>
          x.CreateAsync(It.IsAny<DbLeaveTime>()))
        .ReturnsAsync(_createdLeaveTime.Id);

      _mocker
        .Setup<IDbLeaveTimeMapper, DbLeaveTime>(x =>
          x.Map(It.IsAny<CreateLeaveTimeRequest>(), It.IsAny<double?>(), It.IsAny<string>()))
        .Returns(_createdLeaveTime);

      _mocker
        .Setup<IHttpContextAccessor, IDictionary<object, object>>(x =>
          x.HttpContext.Items)
        .Returns(_itemsWithOwner);
      _mocker
        .Setup<IHttpContextAccessor, int>(x =>
          x.HttpContext.Response.StatusCode)
        .Returns((int)HttpStatusCode.Created);

      _mocker
        .Setup<IWorkTimeMonthLimitRepository, Task<DbWorkTimeMonthLimit>>(x =>
          x.GetAsync(It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync(_monthLimit);

      _mocker
        .Setup<ICalendar, Task<string>>(x =>
          x.GetWorkCalendarByMonthAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
        .ReturnsAsync(Holidays);
    }

    [Test]
    public async Task ShouldReturnForbiddenWhenUserHasNoRights()
    {
      _mocker.Setup<IHttpContextAccessor, IDictionary<object, object>>(x =>
          x.HttpContext.Items)
        .Returns(_itemsWithNotOwner);
      _mocker.Setup<ILeaveTimeAccessValidationHelper, Task<bool>>(x =>
          x.HasRightsAsync(It.IsAny<Guid>()))
        .ReturnsAsync(false);

      SerializerAssert.AreEqual(_forbiddenResponse, await _command.ExecuteAsync(_request));

      Verifiable(
        Times.Once(),
        Times.Never(),
        Times.Once(),
        Times.Never(),
        Times.Never(),
        Times.Exactly(2),
        Times.Never(),
        Times.Never());
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenRequestIsNotValidAsync()
    {
      _mocker.Setup<ICreateLeaveTimeRequestValidator, bool>(x =>
          x.ValidateAsync(_request, default).Result.IsValid)
        .Returns(false);

      SerializerAssert.AreEqual(_badRequestResponse, await _command.ExecuteAsync(_request));

      Verifiable(
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Never(),
        Times.Never(),
        Times.Exactly(2),
        Times.Never(),
        Times.Never());
    }

    [Test]
    public async Task SholdReturnBadGatewayIfICalendarThrowsExceptionAsync()
    {
      _mocker.Setup<IWorkTimeMonthLimitRepository, Task<DbWorkTimeMonthLimit>>(x =>
          x.GetAsync(It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync(null as DbWorkTimeMonthLimit);

      _mocker.Setup<ICalendar, Task<string>>(x =>
          x.GetWorkCalendarByMonthAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
        .ThrowsAsync(new Exception());

      SerializerAssert.AreEqual(_badGatewayResponse, await _command.ExecuteAsync(_prolongedRequest));

      Verifiable(
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Never(),
        Times.Never(),
        Times.Exactly(2),
        Times.Once(),
        Times.Once());
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenRepositoryReturnNullAsync()
    {
      _mocker.Setup<ILeaveTimeRepository, Task<Guid?>>(x =>
          x.CreateAsync(It.IsAny<DbLeaveTime>()))
        .ReturnsAsync((Guid?)null);

      SerializerAssert.AreEqual(_badRequestResponse, await _command.ExecuteAsync(_request));

      Verifiable(
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Exactly(2),
        Times.Never(),
        Times.Never());
    }

    [Test]
    public async Task ShouldCreateLeaveTimeSuccessfullyIfOwnerAsync()
    {
      _mocker.Setup<ILeaveTimeAccessValidationHelper, Task<bool>>(x =>
          x.HasRightsAsync(It.IsAny<Guid>()))
        .ReturnsAsync(false);

      SerializerAssert.AreEqual(_goodResponse, await _command.ExecuteAsync(_request));

      Verifiable(
        Times.Never(),
        Times.Once(),
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Exactly(2),
        Times.Never(),
        Times.Never());
    }

    [Test]
    public async Task ShouldCreateLeaveTimeSuccessfullyIfNotOwnerAndHasRightsAsync()
    {
      _mocker.Setup<IHttpContextAccessor, IDictionary<object, object>>(x =>
          x.HttpContext.Items)
        .Returns(_itemsWithNotOwner);
      _mocker.Setup<ILeaveTimeAccessValidationHelper, Task<bool>>(x =>
          x.HasRightsAsync(It.IsAny<Guid>()))
        .ReturnsAsync(true);

      SerializerAssert.AreEqual(_goodResponse, await _command.ExecuteAsync(_request));

      Verifiable(
        Times.Once(),
        Times.Once(),
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Exactly(4),
        Times.Never(),
        Times.Never());
    }

    [Test]
    public async Task CreateProlongedLeaveTimeSuccessfullyUsingHolidaysFromRepositoryAsync()
    {
      SerializerAssert.AreEqual(_goodResponse, await _command.ExecuteAsync(_prolongedRequest));

      Verifiable(
        Times.Never(),
        Times.Once(),
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Exactly(2),
        Times.Once(),
        Times.Never());
    }

    [Test]
    public async Task CreateProlongedLeaveTimeSuccessfullyUsingHolidaysFromCalendarAsync()
    {
      _mocker.Setup<IWorkTimeMonthLimitRepository, Task<DbWorkTimeMonthLimit>>(x =>
          x.GetAsync(It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync(default(DbWorkTimeMonthLimit));

      SerializerAssert.AreEqual(_goodResponse, await _command.ExecuteAsync(_prolongedRequest));

      Verifiable(
        Times.Never(),
        Times.Once(),
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Exactly(2),
        Times.Once(),
        Times.Once());
    }
  }
}
