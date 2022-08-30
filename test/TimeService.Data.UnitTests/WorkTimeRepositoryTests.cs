using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Moq.AutoMock;
using NUnit.Framework;

namespace LT.DigitalOffice.TimeService.Data.UnitTests
{
  public class CreateWorkTimeTests
  {
    private readonly Dictionary<object, object> _items = new() { { "UserId", Guid.NewGuid() } };

    private readonly DbWorkTime _nullDbWorkTime = null;
    private readonly List<DbWorkTime> _nullListDbWorkTime = null;

    private AutoMocker _mocker;
    private TimeServiceDbContext _dbContext;
    private IHttpContextAccessor _httpContextAccessor;
    private IWorkTimeRepository _repository;

    private DbWorkTime _firstWorkTime;
    private DbWorkTime _secondWorkTime;
    private DbWorkTime _thirdWorkTime;
    private DbWorkTime _editableWorkTime;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      _firstWorkTime = new DbWorkTime
      {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        ProjectId = Guid.NewGuid(),
        Year = 2022,
        Month = 8,
        Hours = 7,
        Description = "Description 1"
      };
      _secondWorkTime = new DbWorkTime
      {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        ProjectId = Guid.NewGuid(),
        Year = 2021,
        Month = 8,
        Hours = 7,
        Description = "Description 2"
      };
      _thirdWorkTime = new DbWorkTime
      {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        Year = 2020,
        Month = 8,
        Hours = 7,
        Description = "Description 3"
      };
      _editableWorkTime = new DbWorkTime
      {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        Year = 2020,
        Month = 8,
        Hours = 7,
        Description = "Description 3"
      };
    }

    [SetUp]
    public void SetUp()
    {
      var dbOptions = new DbContextOptionsBuilder<TimeServiceDbContext>()
        .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
        .Options;

      _dbContext = new TimeServiceDbContext(dbOptions);
      _mocker = new AutoMocker();
      _httpContextAccessor = _mocker.CreateInstance<HttpContextAccessor>();
      _repository = new WorkTimeRepository(_dbContext, _httpContextAccessor);
    }

    [TearDown]
    public void CleanDb()
    {
      if (_dbContext.Database.IsInMemory())
      {
        _dbContext.Database.EnsureDeleted();
      }
    }

    [Test]
    public async Task CreateSuccessfullyAsync()
    {
      Assert.AreEqual(_firstWorkTime.Id, await _repository.CreateAsync(_firstWorkTime));
      Assert.AreEqual(1, await _dbContext.WorkTimes.CountAsync());
    }

    [Test]
    public async Task CreateListSuccessfullyAsync()
    {
      await _repository.CreateAsync(new List<DbWorkTime> { _firstWorkTime, _secondWorkTime });

      Assert.AreEqual(2, await _dbContext.WorkTimes.CountAsync());
    }

    [Test]
    public async Task CreateNothingWithNullListAsync()
    {
      await _repository.CreateAsync(_nullListDbWorkTime);

      Assert.AreEqual(0, await _dbContext.WorkTimes.CountAsync());
    }

    [Test]
    public async Task CreateReturnNullAsync()
    {
      Assert.AreEqual(null, await _repository.CreateAsync(_nullDbWorkTime));
    }

    [Test]
    public async Task GetByIdSuccessfullyAsync()
    {
      await _dbContext.WorkTimes.AddAsync(_firstWorkTime);
      await _dbContext.SaveChangesAsync();

      Assert.AreEqual(_firstWorkTime, await _repository.GetAsync(_firstWorkTime.Id));
    }

    [Test]
    public async Task GetByIdNothingAsync()
    {
      await _dbContext.WorkTimes.AddAsync(_firstWorkTime);
      await _dbContext.SaveChangesAsync();

      Assert.AreEqual(null, await _repository.GetAsync(_secondWorkTime.Id));
    }

    [Test]
    public async Task GetByUsersIdsSuccessfullyAsync()
    {
      await _dbContext.WorkTimes.AddRangeAsync(_firstWorkTime, _secondWorkTime, _thirdWorkTime);
      await _dbContext.SaveChangesAsync();

      List<DbWorkTime> expectedResult = new() { _firstWorkTime };

      Assert.AreEqual(
        expectedResult,
        await _repository.GetAsync(
          new List<Guid> { _firstWorkTime.UserId, _secondWorkTime.UserId},
          null,
          2022,
          8));
    }

    [Test]
    public async Task GetByUsersIdsAndProjectIdsAsync()
    {
      await _dbContext.WorkTimes.AddRangeAsync(_firstWorkTime, _secondWorkTime, _thirdWorkTime);
      await _dbContext.SaveChangesAsync();

      List<DbWorkTime> expectedResult = new();

      Assert.AreEqual(
        expectedResult,
        await _repository.GetAsync(
          new List<Guid> { _firstWorkTime.UserId, _secondWorkTime.UserId },
          new List<Guid> { _secondWorkTime.ProjectId },
          2022,
          null));
    }

    [Test]
    public async Task GetByUsersIdsReturnNullAsync()
    {
      Assert.AreEqual(
        null,
        await _repository.GetAsync(
          null,
          null,
          2022,
          8));
    }

    [Test]
    public async Task FindSuccessfullyAsync()
    {
      await _dbContext.WorkTimes.AddRangeAsync(_firstWorkTime, _secondWorkTime);
      await _dbContext.SaveChangesAsync();

      FindWorkTimesFilter filter = new() { Year = 2022 };
      (List<DbWorkTime> workTimes, int totalCount) expectedWorkTimes = new()
      {
        workTimes = new List<DbWorkTime>
        {
          _firstWorkTime
        },
        totalCount = 1
      };

      Assert.AreEqual(expectedWorkTimes, await _repository.FindAsync(filter));
    }

    [Test]
    public async Task FindNothingAsync()
    {
      await _dbContext.WorkTimes.AddRangeAsync(_firstWorkTime, _secondWorkTime);
      await _dbContext.SaveChangesAsync();

      FindWorkTimesFilter filter = new() { Year = 2023 };
      (List<DbWorkTime> workTimes, int totalCount) expectedWorkTimes = new()
      {
        workTimes = new List<DbWorkTime>(),
        totalCount = 0
      };

      Assert.AreEqual(expectedWorkTimes, await _repository.FindAsync(filter));
    }

    [Test]
    public async Task EditSuccessfullyAsync()
    {
      WorkTimeRepository mockedRepository = _mocker.CreateInstance<WorkTimeRepository>();
      _mocker.Setup<IHttpContextAccessor, IDictionary<object, object>>(x =>
          x.HttpContext.Items)
        .Returns(_items);

      await _dbContext.WorkTimes.AddAsync(_editableWorkTime);
      await _dbContext.SaveChangesAsync();

      string value = "Edited Description";
      JsonPatchDocument<DbWorkTime> request = new()
      {
        Operations = { new() { op = "replace", path = "/Description", value = value } }
      };

      Assert.IsTrue(await mockedRepository.EditAsync(_editableWorkTime, request));
      Assert.AreEqual(value, (await _dbContext.WorkTimes.FirstAsync()).Description);
    }

    [Test]
    public async Task EditReturnNullByWorkTimeAsync()
    {
      Assert.IsFalse(await _repository.EditAsync(null, new JsonPatchDocument<DbWorkTime>()));
    }

    [Test]
    public async Task EditReturnNullByPatchAsync()
    {
      Assert.IsFalse(await _repository.EditAsync(_firstWorkTime, null));
    }

    [Test]
    public async Task GetLastSuccessfullyAsync()
    {
      await _dbContext.WorkTimes.AddRangeAsync(_firstWorkTime, _secondWorkTime);
      await _dbContext.SaveChangesAsync();

      Assert.AreEqual(_firstWorkTime, await _repository.GetLastAsync());
    }

    [Test]
    public async Task DoesExistAsync()
    {
      await _dbContext.WorkTimes.AddAsync(_firstWorkTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsTrue(await _repository.DoesExistAsync(_firstWorkTime.Id));
    }

    [Test]
    public async Task DoesNotExistAsync()
    {
      await _dbContext.WorkTimes.AddAsync(_firstWorkTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsFalse(await _repository.DoesExistAsync(_secondWorkTime.Id));
    }

    [Test]
    public async Task DoesEmptyWorkTimeExistAsync()
    {
      await _dbContext.WorkTimes.AddAsync(_thirdWorkTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsTrue(await _repository.DoesEmptyWorkTimeExistAsync(
        _thirdWorkTime.UserId,
        _thirdWorkTime.Month,
        _thirdWorkTime.Year));
    }

    [Test]
    public async Task DoesNotEmptyWorkTimeExistAsync()
    {
      await _dbContext.WorkTimes.AddAsync(_firstWorkTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsFalse(await _repository.DoesEmptyWorkTimeExistAsync(
        _firstWorkTime.UserId,
        _firstWorkTime.Month,
        _firstWorkTime.Year));
    }
  }
}
