using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;

namespace LT.DigitalOffice.TimeService.Data.UnitTests
{
  public class WorkTimeMonthLimitRepositoryTests
  {
    private readonly Dictionary<object, object> _items = new() { { "UserId", Guid.NewGuid() } };

    private AutoMocker _mocker;
    private TimeServiceDbContext _dbContext;
    private IWorkTimeMonthLimitRepository _repository;
    private IHttpContextAccessor _httpContextAccessor;

    private DbWorkTimeMonthLimit _firstWorkTimeMonthLimit;
    private DbWorkTimeMonthLimit _secondWorkTimeMonthLimit;
    private DbWorkTimeMonthLimit _thirdWorkTimeMonthLimit;
    private DbWorkTimeMonthLimit _editableWorkTimeMonthLimit;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      _firstWorkTimeMonthLimit = new DbWorkTimeMonthLimit
      {
        Id = Guid.NewGuid(),
        Month = 7,
        Year = 2020,
        NormHours = 5f,
        Holidays = "Holidays 1"
      };
      _secondWorkTimeMonthLimit = new DbWorkTimeMonthLimit
      {
        Id = Guid.NewGuid(),
        Month = 8,
        Year = 2021,
        NormHours = 6f,
        Holidays = "Holidays 2"
      };
      _thirdWorkTimeMonthLimit = new DbWorkTimeMonthLimit
      {
        Id = Guid.NewGuid(),
        Month = 9,
        Year = 2022,
        NormHours = 7f,
        Holidays = "Holidays 3"
      };
      _editableWorkTimeMonthLimit = new DbWorkTimeMonthLimit
      {
        Id = Guid.NewGuid(),
        Month = 9,
        Year = 2022,
        NormHours = 7f,
        Holidays = "Holidays 4"
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
      _repository = new WorkTimeMonthLimitRepository(_dbContext, _httpContextAccessor);
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
      Assert.AreEqual(_firstWorkTimeMonthLimit.Id, await _repository.CreateAsync(_firstWorkTimeMonthLimit));
      Assert.AreEqual(1, await _dbContext.WorkTimeMonthLimits.CountAsync());
    }

    [Test]
    public async Task CreateReturnNullAsync()
    {
      Assert.IsNull(await _repository.CreateAsync(null));
      Assert.AreEqual(0, await _dbContext.WorkTimeMonthLimits.CountAsync());

    }

    [Test]
    public async Task CreateRangeSuccessfullyAsync()
    {
      List<DbWorkTimeMonthLimit> dbWorkTimeMonthLimits = new()
      {
        _firstWorkTimeMonthLimit, _secondWorkTimeMonthLimit, _thirdWorkTimeMonthLimit
      };

      await _repository.CreateRangeAsync(dbWorkTimeMonthLimits);

      Assert.AreEqual(3, await _dbContext.WorkTimeMonthLimits.CountAsync());
    }

    [Test]
    public async Task CreateRangeReturnNullAsync()
    {
      await _repository.CreateRangeAsync(null);

      Assert.AreEqual(0, await _dbContext.WorkTimeMonthLimits.CountAsync());
    }

    [Test]
    public async Task CreateRangeReturnNullByOneElementAsync()
    {
      List<DbWorkTimeMonthLimit> dbWorkTimeMonthLimits = new()
      {
        _firstWorkTimeMonthLimit, null, _thirdWorkTimeMonthLimit
      };

      await _repository.CreateRangeAsync(dbWorkTimeMonthLimits);

      Assert.AreEqual(0, await _dbContext.WorkTimeMonthLimits.CountAsync());
    }

    [Test]
    public async Task EditSuccessfullyAsync()
    {
      List<DbWorkTimeMonthLimit> list = new() { _editableWorkTimeMonthLimit };
      IQueryable<DbWorkTimeMonthLimit> queryable = list.AsQueryable();
      Mock<DbSet<DbWorkTimeMonthLimit>> dbSet = new Mock<DbSet<DbWorkTimeMonthLimit>>();

      dbSet.As<IQueryable<DbWorkTimeMonthLimit>>()
        .Setup(x => x.Provider)
        .Returns(queryable.Provider);
      dbSet.As<IQueryable<DbWorkTimeMonthLimit>>()
        .Setup(m => m.Expression)
        .Returns(queryable.Expression);
      dbSet
        .As<IQueryable<DbWorkTimeMonthLimit>>()
        .Setup(m => m.ElementType)
        .Returns(queryable.ElementType);
      dbSet
        .As<IQueryable<DbWorkTimeMonthLimit>>()
        .Setup(m => m.GetEnumerator())
        .Returns(() => queryable.GetEnumerator());

      _mocker
        .Setup<IHttpContextAccessor, IDictionary<object, object>>(x => x.HttpContext.Items)
        .Returns(_items);
      _mocker
        .Setup<IDataProvider, IEnumerable<DbWorkTimeMonthLimit>>(x => x.WorkTimeMonthLimits)
        .Returns(dbSet.Object);

      WorkTimeMonthLimitRepository mockedRepository = _mocker.CreateInstance<WorkTimeMonthLimitRepository>();

      string value = "Edited Holidays";
      JsonPatchDocument<DbWorkTimeMonthLimit> request = new()
      {
        Operations = { new() { op = "replace", path = "/Holidays", value = value } }
      };

      Assert.IsTrue(await mockedRepository.EditAsync(_editableWorkTimeMonthLimit.Id, request));
      Assert.AreEqual(value, _editableWorkTimeMonthLimit.Holidays);
    }

    [Test]
    public async Task EditReturnFalseAsync()
    {
      await _dbContext.AddAsync(_firstWorkTimeMonthLimit);
      await _dbContext.SaveChangesAsync();

      Assert.IsFalse(await _repository.EditAsync(_secondWorkTimeMonthLimit.Id, null));
    }

    [Test]
    public async Task EditReturnFalseByRequestAsync()
    {
      await _dbContext.AddAsync(_firstWorkTimeMonthLimit);
      await _dbContext.SaveChangesAsync();

      Assert.IsFalse(await _repository.EditAsync(_firstWorkTimeMonthLimit.Id, null));
    }

    [Test]
    public async Task FindSuccessfullyAsync()
    {
      await _dbContext.AddRangeAsync(
        _firstWorkTimeMonthLimit,
        _secondWorkTimeMonthLimit,
        _thirdWorkTimeMonthLimit);
      await _dbContext.SaveChangesAsync();

      FindWorkTimeMonthLimitsFilter filter = new() { Year = 2022 };
      (List<DbWorkTimeMonthLimit> dbWorkTimeMonthLimit, int totalCount) expectedResult = new()
      {
        dbWorkTimeMonthLimit = new List<DbWorkTimeMonthLimit> { _thirdWorkTimeMonthLimit },
        totalCount = 1
      };

      Assert.AreEqual(expectedResult, await _repository.FindAsync(filter));
    }

    [Test]
    public async Task FindNothingAsync()
    {
      await _dbContext.AddRangeAsync(
        _firstWorkTimeMonthLimit,
        _secondWorkTimeMonthLimit,
        _thirdWorkTimeMonthLimit);
      await _dbContext.SaveChangesAsync();

      FindWorkTimeMonthLimitsFilter filter = new() { Year = 2023 };
      (List<DbWorkTimeMonthLimit> dbWorkTimeMonthLimit, int totalCount) expectedResult = new()
      {
        dbWorkTimeMonthLimit = new List<DbWorkTimeMonthLimit>(),
        totalCount = 0
      };

      Assert.AreEqual(expectedResult, await _repository.FindAsync(filter));
    }

    [Test]
    public async Task FindReturnNullAsync()
    {
      (List<DbWorkTimeMonthLimit> dbWorkTimeMonthLimit, int totalCount) expectedResult = (null, default);

      Assert.AreEqual(expectedResult, await _repository.FindAsync(null));
    }

    [Test]
    public async Task GetByYearAndMonthSuccessfullyAsync()
    {
      await _dbContext.AddAsync(_firstWorkTimeMonthLimit);
      await _dbContext.SaveChangesAsync();

      Assert.AreEqual(
        _firstWorkTimeMonthLimit,
        await _repository.GetAsync(_firstWorkTimeMonthLimit.Year, _firstWorkTimeMonthLimit.Month));
    }

    [Test]
    public async Task GetByYearAndMonthNothingAsync()
    {
      await _dbContext.AddAsync(_firstWorkTimeMonthLimit);
      await _dbContext.SaveChangesAsync();

      Assert.IsNull(await _repository.GetAsync(_secondWorkTimeMonthLimit.Year, _secondWorkTimeMonthLimit.Month));
    }

    [Test]
    public async Task GetLastSuccessfullyAsync()
    {
      await _dbContext.AddRangeAsync(
        _firstWorkTimeMonthLimit,
        _secondWorkTimeMonthLimit,
        _thirdWorkTimeMonthLimit);
      await _dbContext.SaveChangesAsync();

      Assert.AreEqual(_thirdWorkTimeMonthLimit, await _repository.GetLastAsync());
    }
  }
}
