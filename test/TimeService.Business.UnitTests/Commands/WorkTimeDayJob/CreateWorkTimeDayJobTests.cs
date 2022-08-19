using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTimeDayJob;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTimeDayJob.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTimeDayJob.Interfaces;
using LT.DigitalOffice.UnitTestKernel;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;

namespace LT.DigitalOffice.TimeService.Business.UnitTests.Commands.WorkTimeDayJob
{
  internal class CreateWorkTimeDayJobTests
  {
    private readonly Guid _createdBy = Guid.NewGuid();

    private AutoMocker _mocker;

    private ICreateWorkTimeDayJobCommand _command;
    private CreateWorkTimeDayJobRequest _request;
    private DbWorkTimeDayJob _createdWorkTimeDayJob;
    private OperationResultResponse<Guid?> _badResponse;
    private OperationResultResponse<Guid?> _goodResponse;
    private Dictionary<object, object> _items;
    private DbWorkTime _workTime;

    private void Verifiable(
      Times workTimeRepositoryTimes,
      Times workTimeDayJobRepositoryTimes,
      Times accessValidatorTimes,
      Times responseCreatorTimes,
      Times mapperTimes,
      Times requestValidatorTimes,
      Times httpContextAccessorTimes)
    {
      _mocker.Verify<IWorkTimeRepository>(x =>
          x.GetAsync(_request.WorkTimeId),
        workTimeRepositoryTimes);
      _mocker.Verify<IWorkTimeDayJobRepository>(x =>
          x.CreateAsync(_createdWorkTimeDayJob),
        workTimeDayJobRepositoryTimes);

      _mocker.Verify<IAccessValidator>(x =>
          x.HasRightsAsync(Rights.AddEditRemoveTime),
        accessValidatorTimes);

      _mocker.Verify<IResponseCreator>(x =>
          x.CreateFailureResponse<Guid?>(It.IsAny<HttpStatusCode>(), It.IsAny<List<string>>()),
        responseCreatorTimes);

      _mocker.Verify<IDbWorkTimeDayJobMapper>(x =>
          x.Map(_request),
        mapperTimes);

      _mocker.Verify<ICreateWorkTimeDayJobRequestValidator, bool>(x =>
          x.ValidateAsync(_request, default).Result.IsValid,
        requestValidatorTimes);

      _mocker.Verify<IHttpContextAccessor>(x =>
          x.HttpContext.Items,
        httpContextAccessorTimes);

      _mocker.Resolvers.Clear();
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      _request = new CreateWorkTimeDayJobRequest { WorkTimeId = Guid.NewGuid() };
      _workTime = new DbWorkTime { Id = _request.WorkTimeId };

      _createdWorkTimeDayJob = new DbWorkTimeDayJob { Id = Guid.NewGuid(), WorkTime = _workTime };

      _badResponse = new OperationResultResponse<Guid?> { Errors = new List<string> { "Error" } };
      _goodResponse = new OperationResultResponse<Guid?> { Body = _createdWorkTimeDayJob.Id };

      _items = new() { { "UserId", _createdBy } };
    }

    [SetUp]
    public void SetUp()
    {
      _mocker = new AutoMocker();
      _command = _mocker.CreateInstance<CreateWorkTimeDayJobCommand>();

      _mocker.Setup<IWorkTimeRepository, Task<DbWorkTime>>(x =>
          x.GetAsync(_request.WorkTimeId))
        .ReturnsAsync(_workTime);
      _mocker.Setup<IWorkTimeDayJobRepository, Task<Guid?>>(x =>
          x.CreateAsync(_createdWorkTimeDayJob))
        .ReturnsAsync(_createdWorkTimeDayJob.Id);

      _mocker.Setup<IAccessValidator, Task<bool>>(x =>
          x.HasRightsAsync(Rights.AddEditRemoveTime))
        .ReturnsAsync(true);

      _mocker.Setup<IResponseCreator, OperationResultResponse<Guid?>>(x =>
          x.CreateFailureResponse<Guid?>(It.IsAny<HttpStatusCode>(), It.IsAny<List<string>>()))
        .Returns(_badResponse);

      _mocker.Setup<IDbWorkTimeDayJobMapper, DbWorkTimeDayJob>(x =>
          x.Map(_request))
        .Returns(_createdWorkTimeDayJob);

      _mocker.Setup<ICreateWorkTimeDayJobRequestValidator, bool>(x =>
          x.ValidateAsync(_request, default).Result.IsValid)
        .Returns(true);

      _mocker.Setup<IHttpContextAccessor, IDictionary<object, object>>(x =>
          x.HttpContext.Items)
        .Returns(_items);
      _mocker.Setup<IHttpContextAccessor, int>(x =>
          x.HttpContext.Response.StatusCode)
        .Returns((int)HttpStatusCode.Created);
    }

    [Test]
    public async Task FailAuthorCheckAsync()
    {
      Guid anotherUserId = Guid.NewGuid();
      Dictionary<object, object> otherItems = new() { { "UserId", anotherUserId } };

      _mocker.Setup<IHttpContextAccessor, IDictionary<object, object>>(x =>
          x.HttpContext.Items)
        .Returns(otherItems);
      _mocker.Setup<IAccessValidator, Task<bool>>(x =>
          x.HasRightsAsync(Rights.AddEditRemoveTime))
        .ReturnsAsync(false);

      SerializerAssert.AreEqual(_badResponse, await _command.ExecuteAsync(_request));

      Verifiable(
        Times.Once(),
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Never(),
        Times.Never(),
        Times.Exactly(2));
    }

    [Test]
    public async Task FailValidationAsync()
    {
      _mocker.Setup<ICreateWorkTimeDayJobRequestValidator, bool>(x =>
          x.ValidateAsync(_request, default).Result.IsValid)
        .Returns(false);

      SerializerAssert.AreEqual(_badResponse, await _command.ExecuteAsync(_request));

      Verifiable(
        Times.Once(),
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Never(),
        Times.Once(),
        Times.Exactly(2));
    }

    [Test]
    public async Task FailRepositoryAsync()
    {
      _mocker.Setup<IWorkTimeDayJobRepository, Task<Guid?>>(x =>
          x.CreateAsync(_createdWorkTimeDayJob))
        .ReturnsAsync((Guid?)null);

      SerializerAssert.AreEqual(_badResponse, await _command.ExecuteAsync(_request));

      Verifiable(
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Exactly(2));
    }

    [Test]
    public async Task CreateWorkTimeDayJobSuccessfullyAsync()
    {
      SerializerAssert.AreEqual(_goodResponse, await _command.ExecuteAsync(_request));

      Verifiable(
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Exactly(2));
    }
  }
}
