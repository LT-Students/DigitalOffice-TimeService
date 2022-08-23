using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.Models.Broker.Models.Image;
using LT.DigitalOffice.Models.Broker.Models.Position;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.TimeService.Broker.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Business.Commands.Stat;
using LT.DigitalOffice.TimeService.Business.Commands.Stat.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using LT.DigitalOffice.TimeService.Validation.Stat.Interfaces;
using LT.DigitalOffice.UnitTestKernel;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;

namespace LT.DigitalOffice.TimeService.Business.UnitTests.Commands.Stat
{
  internal class FindStatCommandTests
  {
    private readonly List<ProjectUserData> _nullProjectUserData = default;

    private AutoMocker _mocker;
    private IFindStatCommand _command;

    private FindStatFilter _filter;
    private FindStatFilter _filterWithoutProjectId;
    private FindResultResponse<UserStatInfo> _goodResponse;
    private FindResultResponse<UserStatInfo> _badResponse;
    private Dictionary<object, object> _items;

    private List<ProjectUserData> _projectUserData;
    private List<DepartmentData> _departmentData;
    private List<DepartmentUserExtendedData> _departmentUserExtendedData;
    private List<DbWorkTime> _dbWorkTimes;
    private List<DbLeaveTime> _dbLeaveTimes;
    private List<UserData> _usersData;
    private List<ProjectData> _projectsData;
    private DbWorkTimeMonthLimit _monthLimit;
    private (List<UserData> usersData, int totalCount) _filteredUsersData;
    private List<CompanyData> _companyData;
    private List<ImageData> _imageData;
    private List<PositionData> _positionData;
    private UserInfo _userInfo;
    private UserInfo _managerInfo;
    private ProjectInfo _projectInfo;
    private UserStatInfo _userStatInfo;
    private ProjectUserRoleType? _userRoleType;

    private void Verifiable(
      Times responseCreatorTimes,
      Times httpContextAccessorTimes,
      Times accessValidatorTimes,
      Times getProjectUsersTimes,
      Times getProjectUserRoleTimes,
      Times getProjectsDataTimes,
      Times getDepartmentsDataTimes,
      Times getDepartmentsUsersTimes,
      Times getUsersDataTimes,
      Times getFilteredUsersDataTimes,
      Times companyServiceTimes,
      Times imageServiceTimes,
      Times positionServiceTimes,
      Times workTimeRepositoryTimes,
      Times leaveTimeRepositoryTimes,
      Times workTimeMonthLimitRepositoryTimes,
      Times userInfoMapperTimes,
      Times projectInfoMapperTimes,
      Times userStatInfoMapperTimes,
      Times findStatFilterValidatorTimes)
    {
      _mocker.Verify<IResponseCreator>(x => x.CreateFailureFindResponse<UserStatInfo>(
          It.IsAny<HttpStatusCode>(),
          It.IsAny<List<string>>()),
        responseCreatorTimes);

      _mocker.Verify<IHttpContextAccessor>(x => x.HttpContext.Items,
        httpContextAccessorTimes);

      _mocker.Verify<IAccessValidator>(x =>
          x.HasRightsAsync(Rights.AddEditRemoveTime),
        accessValidatorTimes);

      _mocker.Verify<IProjectService>(x => x.GetProjectsUsersAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<Guid>>(),
          It.IsAny<DateTime>(),
          It.IsAny<List<string>>()),
        getProjectUsersTimes);
      _mocker.Verify<IProjectService>(x => x.GetProjectUserRoleAsync(
          It.IsAny<Guid>(),
          It.IsAny<Guid>(),
          It.IsAny<List<string>>()),
        getProjectUserRoleTimes);
      _mocker.Verify<IProjectService>(x => x.GetProjectsDataAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<Guid>>(),
          It.IsAny<bool>(),
          It.IsAny<bool>(),
          It.IsAny<List<string>>()),
        getProjectsDataTimes);

      _mocker.Verify<IDepartmentService>(x => x.GetDepartmentsDataAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<Guid>>(),
          It.IsAny<List<string>>()),
        getDepartmentsDataTimes);
      _mocker.Verify<IDepartmentService>(x => x.GetDepartmentsUsersAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<DateTime>(),
          It.IsAny<List<string>>()),
        getDepartmentsUsersTimes);

      _mocker.Verify<IUserService>(x => x.GetUsersDataAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<string>>()),
        getUsersDataTimes);
      _mocker.Verify<IUserService>(x => x.GetFilteredUsersDataAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<bool>(),
          It.IsAny<string>(),
          It.IsAny<List<string>>()),
        getFilteredUsersDataTimes);

      _mocker.Verify<ICompanyService>(x => x.GetCompaniesDataAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<string>>()),
        companyServiceTimes);

      _mocker.Verify<IImageService>(x => x.GetUsersImagesAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<string>>()),
        imageServiceTimes);

      _mocker.Verify<IPositionService>(x => x.GetPositionsAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<string>>()),
        positionServiceTimes);

      _mocker.Verify<IWorkTimeRepository>(x => x.GetAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<Guid>>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<bool>()),
        workTimeRepositoryTimes);

      _mocker.Verify<ILeaveTimeRepository>(x => x.GetAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<bool>()),
        leaveTimeRepositoryTimes);

      _mocker.Verify<IWorkTimeMonthLimitRepository>(x => x.GetAsync(
          It.IsAny<int>(),
          It.IsAny<int>()),
        workTimeMonthLimitRepositoryTimes);

      _mocker.Verify<IUserInfoMapper>(x => x.Map(
          It.IsAny<UserData>(),
          It.IsAny<ImageData>()),
        userInfoMapperTimes);

      _mocker.Verify<IProjectInfoMapper>(x => x.Map(It.IsAny<ProjectData>()),
        projectInfoMapperTimes);

      _mocker.Verify<IUserStatInfoMapper>(x => x.Map(
          It.IsAny<UserInfo>(),
          It.IsAny<List<UserInfo>>(),
          It.IsAny<DbWorkTimeMonthLimit>(),
          It.IsAny<List<DbWorkTime>>(),
          It.IsAny<List<DbLeaveTime>>(),
          It.IsAny<List<ProjectInfo>>(),
          It.IsAny<PositionData>(),
          It.IsAny<DepartmentData>(),
          It.IsAny<CompanyUserData>()),
        userStatInfoMapperTimes);

      _mocker.Verify<IFindStatFilterValidator>(x => x.ValidateAsync(
          It.IsAny<FindStatFilter>(),
          default),
        findStatFilterValidatorTimes);

      _mocker.Resolvers.Clear();
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      _filter = new FindStatFilter
      {
        DepartmentsIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
        ProjectId = Guid.NewGuid(),
        Year = 2022,
        Month = 10,
        AscendingSort = true
      };
      _filterWithoutProjectId = new FindStatFilter
      {
        DepartmentsIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
        ProjectId = null,
        Year = 2022,
        Month = 10,
        AscendingSort = true
      };
      _projectUserData = new List<ProjectUserData>();
      _departmentData = new List<DepartmentData>();
      _departmentUserExtendedData = new List<DepartmentUserExtendedData>();
      _dbWorkTimes = new List<DbWorkTime>();
      _dbLeaveTimes = new List<DbLeaveTime>();
      _usersData = new List<UserData>();
      _projectsData = new List<ProjectData> { new(Guid.Empty, null, null, null, null, null, null)};
      _monthLimit = new DbWorkTimeMonthLimit();
      _filteredUsersData.usersData = new List<UserData>
      {
        new(Guid.Empty, null, null, null, null, true)
      };
      _filteredUsersData.totalCount = 1;
      _companyData = new List<CompanyData>();
      _imageData = new List<ImageData>();
      _positionData = new List<PositionData>();
      _userInfo = new UserInfo();
      _managerInfo = new UserInfo();
      _projectInfo = new ProjectInfo();
      _userRoleType = ProjectUserRoleType.Manager;
      _userStatInfo = new UserStatInfo();
      _goodResponse = new FindResultResponse<UserStatInfo>
      {
        Body = new List<UserStatInfo> { _userStatInfo }, 
        TotalCount = 1,
        Errors = new List<string>()
      };
      _badResponse = new FindResultResponse<UserStatInfo> { Errors = new List<string> { "Errors" }, TotalCount = 0 };
      _items = new() { { "UserId", Guid.NewGuid() } };
    }

    [SetUp]
    public void SetUp()
    {
      _mocker = new AutoMocker();
      _command = _mocker.CreateInstance<FindStatCommand>();

      _mocker.Setup<IResponseCreator, FindResultResponse<UserStatInfo>>(x => x.CreateFailureFindResponse<UserStatInfo>(
          It.IsAny<HttpStatusCode>(),
          It.IsAny<List<string>>()))
        .Returns(_badResponse);
      _mocker.Setup<IHttpContextAccessor, IDictionary<object, object>>(x => x.HttpContext.Items)
        .Returns(_items);
      _mocker.Setup<IAccessValidator, Task<bool>>(x =>
          x.HasRightsAsync(Rights.AddEditRemoveTime))
        .ReturnsAsync(true);

      _mocker.Setup<IProjectService, Task<List<ProjectUserData>>>(x => x.GetProjectsUsersAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<Guid>>(),
          It.IsAny<DateTime>(),
          It.IsAny<List<string>>()))
        .ReturnsAsync(_projectUserData);
      _mocker.Setup<IProjectService, Task<ProjectUserRoleType?>>(x => x.GetProjectUserRoleAsync(
          It.IsAny<Guid>(),
          It.IsAny<Guid>(),
          It.IsAny<List<string>>()))
        .ReturnsAsync(_userRoleType);
      _mocker.Setup<IProjectService, Task<List<ProjectData>>>(x => x.GetProjectsDataAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<Guid>>(),
          It.IsAny<bool>(),
          It.IsAny<bool>(),
          It.IsAny<List<string>>()))
        .ReturnsAsync(_projectsData);

      _mocker.Setup<IDepartmentService, Task<List<DepartmentData>>>(x => x.GetDepartmentsDataAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<Guid>>(),
          It.IsAny<List<string>>()))
        .ReturnsAsync(_departmentData);
      _mocker.Setup<IDepartmentService, Task<List<DepartmentUserExtendedData>>>(x => x.GetDepartmentsUsersAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<DateTime>(),
          It.IsAny<List<string>>()))
        .ReturnsAsync(_departmentUserExtendedData);

      _mocker.Setup<IUserService, Task<List<UserData>>>(x => x.GetUsersDataAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<string>>()))
        .ReturnsAsync(_usersData);
      _mocker.Setup<IUserService, Task<(List<UserData> usersData, int totalCount)>>(x => x.GetFilteredUsersDataAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<bool>(),
          It.IsAny<string>(),
          It.IsAny<List<string>>()))
        .ReturnsAsync(_filteredUsersData);

      _mocker.Setup<ICompanyService, Task<List<CompanyData>>>(x => x.GetCompaniesDataAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<string>>()))
        .ReturnsAsync(_companyData);

      _mocker.Setup<IImageService, Task<List<ImageData>>>(x => x.GetUsersImagesAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<string>>()))
        .ReturnsAsync(_imageData);

      _mocker.Setup<IPositionService, Task<List<PositionData>>>(x => x.GetPositionsAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<string>>()))
        .ReturnsAsync(_positionData);

      _mocker.Setup<IWorkTimeRepository, Task<List<DbWorkTime>>>(x => x.GetAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<Guid>>(),
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<bool>()))
        .ReturnsAsync(_dbWorkTimes);

      _mocker.Setup<ILeaveTimeRepository, Task<List<DbLeaveTime>>>(x => x.GetAsync(
        It.IsAny<List<Guid>>(),
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<bool>()))
        .ReturnsAsync(_dbLeaveTimes);

      _mocker.Setup<IWorkTimeMonthLimitRepository, Task<DbWorkTimeMonthLimit>>(x => x.GetAsync(
          It.IsAny<int>(),
          It.IsAny<int>()))
        .ReturnsAsync(_monthLimit);

      _mocker.Setup<IUserInfoMapper, UserInfo>(x => x.Map(
          It.IsAny<UserData>(),
          null))
        .Returns(_userInfo);
      _mocker.Setup<IUserInfoMapper, UserInfo>(x => x.Map(
          It.IsAny<UserData>(),
          It.IsAny<ImageData>()))
        .Returns(_managerInfo);

      _mocker.Setup<IProjectInfoMapper, ProjectInfo>(x =>
          x.Map(It.IsAny<ProjectData>()))
        .Returns(_projectInfo);

      _mocker.Setup<IUserStatInfoMapper, UserStatInfo>(x => x.Map(
          It.IsAny<UserInfo>(),
          It.IsAny<List<UserInfo>>(),
          It.IsAny<DbWorkTimeMonthLimit>(),
          It.IsAny<List<DbWorkTime>>(),
          It.IsAny<List<DbLeaveTime>>(),
          It.IsAny<List<ProjectInfo>>(),
          It.IsAny<PositionData>(),
          It.IsAny<DepartmentData>(),
          It.IsAny<CompanyUserData>()))
        .Returns(_userStatInfo);

      _mocker.Setup<IFindStatFilterValidator, bool>(x =>
          x.ValidateAsync(_filter, default).Result.IsValid)
        .Returns(true);
      _mocker.Setup<IFindStatFilterValidator, bool>(x =>
          x.ValidateAsync(_filterWithoutProjectId, default).Result.IsValid)
        .Returns(true);
    }

    [Test]
    public async Task FailValidationAsync()
    {
      _mocker.Setup<IFindStatFilterValidator, bool>(x =>
          x.ValidateAsync(_filter, default).Result.IsValid)
        .Returns(false);

      SerializerAssert.AreEqual(_badResponse, await _command.ExecuteAsync(_filter));

      Verifiable(
        Times.Once(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Once());
    }

    [Test]
    public async Task ProjectServiceReturnNullAsync()
    {
      _mocker.Setup<IProjectService, Task<List<ProjectUserData>>>(x => x.GetProjectsUsersAsync(
          It.IsAny<List<Guid>>(),
          It.IsAny<List<Guid>>(),
          It.IsAny<DateTime>(),
          It.IsAny<List<string>>()))
        .ReturnsAsync(_nullProjectUserData);

      SerializerAssert.AreEqual(_badResponse, await _command.ExecuteAsync(_filter));

      Verifiable(
        Times.Once(),
        Times.Exactly(2),
        Times.Never(),
        Times.Once(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Once());
    }

    [Test]
    public async Task ProjectUserRoleNotManagerAsync()
    {
      _mocker.Setup<IProjectService, Task<ProjectUserRoleType?>>(x => x.GetProjectUserRoleAsync(
          It.IsAny<Guid>(),
          It.IsAny<Guid>(),
          It.IsAny<List<string>>()))
        .ReturnsAsync(ProjectUserRoleType.Observer);
      _mocker.Setup<IAccessValidator, Task<bool>>(x =>
          x.HasRightsAsync(Rights.AddEditRemoveTime))
        .ReturnsAsync(false);

      SerializerAssert.AreEqual(_badResponse, await _command.ExecuteAsync(_filter));

      Verifiable(
        Times.Once(),
        Times.Exactly(2),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Once());
    }

    [Test]
    public async Task SuccessfullyFindUserStatInfoAsync()
    {
      SerializerAssert.AreEqual(_goodResponse, await _command.ExecuteAsync(_filter));

      Verifiable(
        Times.Never(),
        Times.Exactly(2),
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Never(),
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once());
    }

    [Test]
    public async Task FailCheckWithoutProjectIdAsync()
    {
      _mocker.Setup<IAccessValidator, Task<bool>>(x =>
          x.HasRightsAsync(Rights.AddEditRemoveTime))
        .ReturnsAsync(false);

      SerializerAssert.AreEqual(_badResponse, await _command.ExecuteAsync(_filterWithoutProjectId));

      Verifiable(
        Times.Once(),
        Times.Exactly(2),
        Times.Once(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Never(),
        Times.Once());
    }

    [Test]
    public async Task SuccessfullyFindUserStatInfoWithoutProjectIdAsync()
    {
      SerializerAssert.AreEqual(_goodResponse, await _command.ExecuteAsync(_filterWithoutProjectId));

      Verifiable(
        Times.Never(),
        Times.Exactly(2),
        Times.Once(),
        Times.Never(),
        Times.Never(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once(),
        Times.Once());
    }
  }
}
