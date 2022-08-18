using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces;
using LT.DigitalOffice.UnitTestKernel;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;

namespace LT.DigitalOffice.TimeService.Business.UnitTests.Commands.WorkTime
{
  internal class CreateWorkTimeCommandTests
  {
    private AutoMocker _mocker;
    private ICreateWorkTimeCommand _command;

    private CreateWorkTimeRequest _request;
    private DbWorkTime _createdWorkTime;
    private OperationResultResponse<Guid?> _badResponse;
    private OperationResultResponse<Guid?> _goodResponse;

    private void Verifiable(
      Times validatorTimes,
      Times responseCreatorTimes,
      Times repositoryTimes,
      Times mapperTimes)
    {
      _mocker.Verify<ICreateWorkTimeRequestValidator, bool>(x =>
          x.ValidateAsync(_request, default).Result.IsValid,
        validatorTimes);

      _mocker.Verify<IResponseCreator>(x =>
          x.CreateFailureResponse<Guid?>(It.IsAny<HttpStatusCode>(), It.IsAny<List<string>>()),
        responseCreatorTimes);

      _mocker.Verify<IWorkTimeRepository>(x =>
          x.CreateAsync(_createdWorkTime),
        repositoryTimes);

      _mocker.Verify<IDbWorkTimeMapper>(x =>
          x.Map(_request),
        mapperTimes);

      _mocker.Resolvers.Clear();
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      _request = new CreateWorkTimeRequest
      {
        Hours = 100f,
        Description = "Description",
        Year = 2022,
        Month = 10
      };

      _createdWorkTime = new DbWorkTime
      {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        ProjectId = default,
        Year = _request.Year,
        Month = _request.Month,
        Hours = _request.Hours,
        Description = _request.Description
      };

      _badResponse = new OperationResultResponse<Guid?>
      {
        Body = null,
        Errors = new List<string> { "Error" }
      };

      _goodResponse = new OperationResultResponse<Guid?>
      {
        Body = _createdWorkTime.Id
      };
    }

    [SetUp]
    public void SetUp()
    {
      _mocker = new AutoMocker();
      _command = _mocker.CreateInstance<CreateWorkTimeCommand>();

      _mocker.Setup<ICreateWorkTimeRequestValidator, bool>(x =>
          x.ValidateAsync(_request, default).Result.IsValid)
        .Returns(true);

      _mocker.Setup<IResponseCreator, OperationResultResponse<Guid?>>(x =>
          x.CreateFailureResponse<Guid?>(It.IsAny<HttpStatusCode>(), It.IsAny<List<string>>()))
        .Returns(_badResponse);

      _mocker.Setup<IWorkTimeRepository, Task<Guid?>>(x =>
          x.CreateAsync(_createdWorkTime))
        .ReturnsAsync(_createdWorkTime.Id);

      _mocker.Setup<IDbWorkTimeMapper, DbWorkTime>(x =>
          x.Map(_request))
        .Returns(_createdWorkTime);

      _mocker.Setup<IHttpContextAccessor, int>(x =>
          x.HttpContext.Response.StatusCode)
        .Returns((int)HttpStatusCode.Created);
    }

    [Test]
    public async Task FailValidationAsync()
    {
      _mocker.Setup<ICreateWorkTimeRequestValidator, bool>(x =>
          x.ValidateAsync(_request, default).Result.IsValid)
        .Returns(false);

      SerializerAssert.AreEqual(_badResponse, await _command.ExecuteAsync(_request));

      Verifiable(
        Times.Once(),
        Times.Once(),
        Times.Never(),
        Times.Never());
    }

    [Test]
    public async Task FailRepositoryAsync()
    {
      _mocker.Setup<IWorkTimeRepository, Task<Guid?>>(x =>
          x.CreateAsync(_createdWorkTime))
        .ReturnsAsync((Guid?)null);

      SerializerAssert.AreEqual(_badResponse, await _command.ExecuteAsync(_request));

      Verifiable(
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once());
    }

    [Test]
    public async Task CreateWorkTimeSuccessfullyAsync()
    {
      SerializerAssert.AreEqual(_goodResponse, await _command.ExecuteAsync(_request));

      Verifiable(
        Times.Once(),
        Times.Never(),
        Times.Once(),
        Times.Once());
    }
  }
}
