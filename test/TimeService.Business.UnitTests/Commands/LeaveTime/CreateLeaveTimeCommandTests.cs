using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Helpers;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
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

    private CreateLeaveTimeRequest _request;
    private DbLeaveTime _createdLeaveTime;
    private OperationResultResponse<Guid?> _badResponse;
    private OperationResultResponse<Guid?> _goodResponse;
    private Guid _createdBy;
    private Dictionary<object, object> _items;

    private void Verifiable(
      Times ltAccessValidationHelperTimes,
      Times validatorTimes,
      Times responseCreatorTimes,
      Times repositoryTimes,
      Times mapperTimes,
      Times httpContextAccessorItemsTimes)
    {
      _mocker.Verify<ILeaveTimeAccessValidationHelper>(x =>
        x.HasRightsAsync(It.IsAny<Guid>()),
        ltAccessValidationHelperTimes);

      _mocker.Verify<ICreateLeaveTimeRequestValidator, bool>(x =>
          x.ValidateAsync(_request, default).Result.IsValid,
        validatorTimes);

      _mocker.Verify<IResponseCreator>(x =>
        x.CreateFailureResponse<Guid?>(It.IsAny<HttpStatusCode>(), It.IsAny<List<string>>()),
        responseCreatorTimes);

      _mocker.Verify<ILeaveTimeRepository>(x =>
          x.CreateAsync(It.IsAny<DbLeaveTime>()),
        repositoryTimes);

      _mocker.Verify<IDbLeaveTimeMapper>(x =>
          x.Map(It.IsAny<CreateLeaveTimeRequest>()),
        mapperTimes);

      _mocker.Verify<IHttpContextAccessor>(x =>
        x.HttpContext.Items,
        httpContextAccessorItemsTimes);

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
        StartTime = new DateTime(2020, 7, 24),
        EndTime = new DateTime(2020, 7, 27),
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

      _badResponse = new()
      {
        Body = null,
        Errors = new List<string> { "Error" }
      };

      _goodResponse = new()
      {
        Body = _createdLeaveTime.Id,
        Errors = new List<string>()
      };

      _items = new()
      {
        { "UserId", _createdBy }
      };
    }

    [SetUp]
    public void SetUp()
    {
      _mocker = new AutoMocker();
      _command = _mocker.CreateInstance<CreateLeaveTimeCommand>();

      _mocker.Setup<ILeaveTimeAccessValidationHelper, Task<bool>>(x =>
          x.HasRightsAsync(It.IsAny<Guid>()))
        .ReturnsAsync(true);

      _mocker.Setup<ICreateLeaveTimeRequestValidator, bool>(x =>
          x.ValidateAsync(_request,default).Result.IsValid)
        .Returns(true);

      _mocker.Setup<IResponseCreator, OperationResultResponse<Guid?>>(x =>
          x.CreateFailureResponse<Guid?>(It.IsAny<HttpStatusCode>(), It.IsAny<List<string>>()))
        .Returns(_badResponse);

      _mocker.Setup<ILeaveTimeRepository, Task<Guid?>>(x =>
          x.CreateAsync(It.IsAny<DbLeaveTime>()))
        .ReturnsAsync(_createdLeaveTime.Id);

      _mocker.Setup<IDbLeaveTimeMapper, DbLeaveTime>(x =>
          x.Map(It.IsAny<CreateLeaveTimeRequest>()))
        .Returns(_createdLeaveTime);

      _mocker.Setup<IHttpContextAccessor, IDictionary<object, object>>(x =>
          x.HttpContext.Items)
        .Returns(_items);
      _mocker.Setup<IHttpContextAccessor, int>(x =>
          x.HttpContext.Response.StatusCode)
        .Returns((int)HttpStatusCode.Created);
    }

    [Test]
    public async Task FailUserCheckAsync()
    {
      Dictionary<object, object> items = new()
      {
        { "UserId", Guid.NewGuid() }
      };
      _mocker.Setup<IHttpContextAccessor, IDictionary<object, object>>(x =>
          x.HttpContext.Items)
        .Returns(items);
      _mocker.Setup<ILeaveTimeAccessValidationHelper, Task<bool>>(x =>
          x.HasRightsAsync(It.IsAny<Guid>()))
        .ReturnsAsync(false);

      SerializerAssert.AreEqual(_badResponse, await _command.ExecuteAsync(_request));

      Verifiable(
        Times.Once(),
        Times.Never(),
        Times.Once(),
        Times.Never(),
        Times.Never(),
        Times.Exactly(2));
    }

    [Test]
    public async Task FailValidationAsync()
    {
      _mocker.Setup<ICreateLeaveTimeRequestValidator, bool>(x =>
          x.ValidateAsync(_request, default).Result.IsValid)
        .Returns(false);

      SerializerAssert.AreEqual(_badResponse, await _command.ExecuteAsync(_request));

      Verifiable(
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Never(),
        Times.Never(),
        Times.Exactly(2));
    }

    [Test]
    public async Task FailRepositoryCreateAsync()
    {
      _mocker.Setup<ILeaveTimeRepository, Task<Guid?>>(x =>
          x.CreateAsync(It.IsAny<DbLeaveTime>()))
        .ReturnsAsync((Guid?)null);

      SerializerAssert.AreEqual(_badResponse, await _command.ExecuteAsync(_request));

      Verifiable(
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Exactly(2));
    }

    [Test]
    public async Task CreateLeaveTimeSuccessfullyAsync()
    {
      SerializerAssert.AreEqual(_goodResponse, await _command.ExecuteAsync(_request));

      Verifiable(
        Times.Never(),
        Times.Once(),
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Exactly(2));
    }
  }
}
