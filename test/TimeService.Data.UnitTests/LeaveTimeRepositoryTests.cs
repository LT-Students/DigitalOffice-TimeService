using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.UnitTestKernel;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LT.DigitalOffice.TimeService.Data.UnitTests
{
  public class CreateLeaveTimeTests
  {
    private TimeServiceDbContext _dbContext;
    private ILeaveTimeRepository _repository;

    private Guid _firstWorkerId;
    private Guid _secondWorkerId;
    private DbLeaveTime _firstLeaveTime;
    private DbLeaveTime _secondLeaveTime;
    private DbLeaveTime _thirdLeaveTime;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      var dbOptions = new DbContextOptionsBuilder<TimeServiceDbContext>()
                              .UseInMemoryDatabase("InMemoryDatabase")
                              .Options;
      _dbContext = new TimeServiceDbContext(dbOptions);
      _repository = new LeaveTimeRepository(_dbContext);

      _firstWorkerId = Guid.NewGuid();
      _secondWorkerId = Guid.NewGuid();

      _firstLeaveTime = new DbLeaveTime
      {
        Id = Guid.NewGuid(),
        LeaveType = (int)LeaveType.SickLeave,
        Comment = "SickLeave",
        StartTime = new DateTime(2020, 7, 5),
        EndTime = new DateTime(2020, 7, 25),
        UserId = _firstWorkerId,
        IsActive = true
      };
      _secondLeaveTime = new DbLeaveTime
      {
        Id = Guid.NewGuid(),
        LeaveType = (int)LeaveType.Training,
        Comment = "SickLeave",
        StartTime = new DateTime(2020, 7, 10),
        EndTime = new DateTime(2020, 7, 20),
        UserId = _secondWorkerId,
        IsActive = true
      };
      _thirdLeaveTime = new DbLeaveTime
      {
        Id = Guid.NewGuid(),
        LeaveType = (int)LeaveType.SickLeave,
        Comment = "SickLeave",
        StartTime = new DateTime(2020, 7, 5),
        EndTime = new DateTime(2020, 7, 25),
        UserId = _firstWorkerId,
        IsActive = true
      };
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
    public async Task SuccessfullyCreateAsync()
    {
      Assert.AreEqual(_firstLeaveTime.Id, await _repository.CreateAsync(_firstLeaveTime));
      SerializerAssert.AreEqual(_firstLeaveTime, await _dbContext.LeaveTimes.FirstAsync());
    }

    [Test]
    public async Task CreateReturnNullAsync()
    {
      Assert.AreEqual(null, await _repository.CreateAsync(null));
      Assert.AreEqual(0, await _dbContext.LeaveTimes.CountAsync());
    }

    [Test]
    public async Task EditSuccessfullyAsync()
    {
      await _dbContext.AddAsync(_firstLeaveTime);
      await _dbContext.SaveChangesAsync();

      string value = "EditedSickLeave";
      JsonPatchDocument<DbLeaveTime> request = new()
      {
        Operations = { new() { op = "replace", path = "/Comment", value = value } }
      };

      Assert.IsTrue(await _repository.EditAsync(_firstLeaveTime, request));
      Assert.AreEqual(value, (await _dbContext.LeaveTimes.FirstAsync()).Comment);
    }

    [Test]
    public async Task EditReturnFalseAsync()
    {
      Assert.IsFalse(await _repository.EditAsync(null, new JsonPatchDocument<DbLeaveTime>()));
    }

    [Test]
    public async Task FindSuccessfullyAsync()
    {
      await _dbContext.AddRangeAsync(_firstLeaveTime, _secondLeaveTime, _thirdLeaveTime);
      await _dbContext.SaveChangesAsync();

      FindLeaveTimesFilter filter = new() { StartTime = _firstLeaveTime.StartTime };
      (List<DbLeaveTime> leaveTimes, int totalCount) expectedLeaveTimes = new()
      {
        leaveTimes = new List<DbLeaveTime>
        {
          _firstLeaveTime,
          _thirdLeaveTime
        },
        totalCount = 2
      };

      Assert.AreEqual(3, await _dbContext.LeaveTimes.CountAsync());
      SerializerAssert.AreEqual(expectedLeaveTimes, await _repository.FindAsync(filter));
    }

    [Test]
    public async Task FindNothingAsync()
    {
      await _dbContext.AddRangeAsync(_firstLeaveTime, _secondLeaveTime, _thirdLeaveTime);
      await _dbContext.SaveChangesAsync();

      FindLeaveTimesFilter filter = new() { StartTime = new DateTime(2030, 12, 30) };
      (List<DbLeaveTime> leaveTimes, int totalCount) expectedLeaveTimes = new();

      Assert.AreEqual(3, await _dbContext.LeaveTimes.CountAsync());
      SerializerAssert.AreEqual(expectedLeaveTimes, await _repository.FindAsync(filter));
    }

    [Test]
    public async Task GetByYearSuccessfullyAsync()
    {
      await _dbContext.AddAsync(_firstLeaveTime);
      await _dbContext.SaveChangesAsync();

      SerializerAssert.AreEqual(
        new List<DbLeaveTime> { _firstLeaveTime },
        await _repository.GetAsync(new List<Guid> { _firstLeaveTime.UserId }, 2020, null));
    }

    [Test]
    public async Task GetByYearWithMonthSuccessfullyAsync()
    {
      await _dbContext.AddAsync(_firstLeaveTime);
      await _dbContext.SaveChangesAsync();

      SerializerAssert.AreEqual(
        new List<DbLeaveTime> { _firstLeaveTime },
        await _repository.GetAsync(new List<Guid> { _firstLeaveTime.UserId }, 2020, 7));
    }

    [Test]
    public async Task GetByYearWithMonthNothingAsync()
    {
      await _dbContext.AddAsync(_firstLeaveTime);
      await _dbContext.SaveChangesAsync();

      SerializerAssert.AreEqual(
        Enumerable.Empty<DbLeaveTime>(),
        await _repository.GetAsync(new List<Guid> { _firstLeaveTime.UserId }, 2020, 8));
    }

    [Test]
    public async Task GetByYearNothingAsync()
    {
      await _dbContext.AddAsync(_firstLeaveTime);
      await _dbContext.SaveChangesAsync();

      SerializerAssert.AreEqual(
        Enumerable.Empty<DbLeaveTime>(),
        await _repository.GetAsync(new List<Guid> { _secondLeaveTime.UserId }, 2020, null));
    }

    [Test]
    public async Task GetByYearNullWithoutUsersIdsAsync()
    {
      await _dbContext.AddAsync(_firstLeaveTime);
      await _dbContext.SaveChangesAsync();

      SerializerAssert.AreEqual(
        null,
        await _repository.GetAsync(null, 2020, null));
    }

    [Test]
    public async Task GetSuccessfullyAsync()
    {
      await _dbContext.AddAsync(_firstLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.AreEqual(_firstLeaveTime, await _repository.GetAsync(_firstLeaveTime.Id));
    }

    [Test]
    public async Task GetNothingAsync()
    {
      await _dbContext.AddAsync(_firstLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.AreEqual(null, await _repository.GetAsync(_secondLeaveTime.Id));
    }

    [Test]
    public async Task HasOverlapByUserAsync()
    {
      await _dbContext.AddRangeAsync(_firstLeaveTime, _thirdLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsTrue(await _repository.HasOverlapAsync(
        _firstLeaveTime.UserId,
        _firstLeaveTime.StartTime,
        _firstLeaveTime.EndTime));
    }

    [Test]
    public async Task DoNotHaveOverlapByUserAsync()
    {
      await _dbContext.AddRangeAsync(_firstLeaveTime, _thirdLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsFalse(await _repository.HasOverlapAsync(
        _secondLeaveTime.UserId,
        _firstLeaveTime.StartTime,
        _firstLeaveTime.EndTime));
    }

    [Test]
    public async Task HasOverlapByLeaveTimeAsync()
    {
      await _dbContext.AddAsync(_thirdLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsTrue(await _repository.HasOverlapAsync(
        _firstLeaveTime,
        _firstLeaveTime.StartTime,
        _firstLeaveTime.EndTime));
    }

    [Test]
    public async Task DoNotHaveOverlapByLeaveTimeAsync()
    {
      await _dbContext.AddAsync(_thirdLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsFalse(await _repository.HasOverlapAsync(
        _secondLeaveTime,
        _secondLeaveTime.StartTime,
        _secondLeaveTime.EndTime));
    }
  }
}
