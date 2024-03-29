﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Moq.AutoMock;
using NUnit.Framework;

namespace LT.DigitalOffice.TimeService.Data.UnitTests
{
  public class LeaveTimeRepositoryTests
  {
    private AutoMocker _mocker;
    private TimeServiceDbContext _dbContext;
    private ILeaveTimeRepository _repository;

    private Guid _firstWorkerId;
    private Guid _secondWorkerId;
    private DbLeaveTime _firstLeaveTime;
    private DbLeaveTime _secondLeaveTime;
    private DbLeaveTime _thirdLeaveTime;
    private DbLeaveTime _thirdManagerLeaveTime;
    private DbLeaveTime _fourthLeaveTime;
    private DbLeaveTime _editableLeaveTime;
    private DbLeaveTime _openedProlongedLeaveTime;
    private DbLeaveTime _closedProlongedLeaveTime;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      _mocker = new AutoMocker();

      _mocker
        .Setup<IHttpContextAccessor, IDictionary<object, object>>(x =>
          x.HttpContext.Items)
        .Returns(new Dictionary<object, object>()
        {
          { "UserId", Guid.NewGuid() }
        });

      var dbOptions = new DbContextOptionsBuilder<TimeServiceDbContext>()
        .UseInMemoryDatabase("InMemoryDatabase")
        .Options;
      _dbContext = new TimeServiceDbContext(dbOptions);
      _repository = new LeaveTimeRepository(_dbContext, _mocker.GetMock<IHttpContextAccessor>().Object);

      _firstWorkerId = Guid.NewGuid();
      _secondWorkerId = Guid.NewGuid();

      _firstLeaveTime = new DbLeaveTime
      {
        Id = Guid.NewGuid(),
        LeaveType = (int)LeaveType.SickLeave,
        Comment = "SickLeave 1",
        StartTime = new DateTime(2020, 7, 5),
        EndTime = new DateTime(2020, 7, 25),
        UserId = _firstWorkerId,
        IsClosed = true,
        IsActive = true
      };
      _secondLeaveTime = new DbLeaveTime
      {
        Id = Guid.NewGuid(),
        LeaveType = (int)LeaveType.Training,
        Comment = "SickLeave 2",
        StartTime = new DateTime(2020, 7, 10),
        EndTime = new DateTime(2020, 7, 20),
        UserId = _secondWorkerId,
        IsClosed = true,
        IsActive = true
      };
      _thirdLeaveTime = new DbLeaveTime
      {
        Id = Guid.NewGuid(),
        LeaveType = (int)LeaveType.SickLeave,
        Comment = "SickLeave 3",
        StartTime = new DateTime(2022, 7, 5),
        EndTime = new DateTime(2022, 7, 25),
        UserId = _firstWorkerId,
        IsClosed = true,
        IsActive = true
      };
      _thirdManagerLeaveTime = new()
      {
        Id = Guid.NewGuid(),
        ParentId = _thirdLeaveTime.Id,
        LeaveType = (int)LeaveType.SickLeave,
        Comment = "SickLeave 3",
        StartTime = new DateTime(2020, 7, 5),
        EndTime = new DateTime(2020, 7, 25),
        UserId = _firstWorkerId,
        IsClosed = true,
        IsActive = true
      };
      _fourthLeaveTime = new DbLeaveTime
      {
        Id = Guid.NewGuid(),
        LeaveType = (int)LeaveType.SickLeave,
        Comment = "SickLeave 4",
        StartTime = new DateTime(2020, 6, 5),
        EndTime = new DateTime(2020, 6, 25),
        UserId = _firstWorkerId,
        IsClosed = true,
        IsActive = true
      };
      _editableLeaveTime = new DbLeaveTime
      {
        Id = Guid.NewGuid(),
        LeaveType = (int)LeaveType.SickLeave,
        Comment = "SickLeave 4",
        StartTime = new DateTime(2020, 7, 5),
        EndTime = new DateTime(2020, 7, 25),
        UserId = _firstWorkerId,
        IsClosed = true,
        IsActive = true
      };
      _openedProlongedLeaveTime = new()
      {
        Id = Guid.NewGuid(),
        LeaveType = (int)LeaveType.Prolonged,
        Comment = "Opened prolonged",
        StartTime = new DateTime(2020, 7, 6),
        EndTime = new DateTime(2020, 7, 1).AddMonths(1).AddMilliseconds(-1),
        UserId = _firstWorkerId,
        IsClosed = false,
        IsActive = true
      };
      _closedProlongedLeaveTime = new()
      {
        Id = Guid.NewGuid(),
        LeaveType = (int)LeaveType.Prolonged,
        Comment = "Opened prolonged",
        StartTime = new DateTime(2020, 6, 5),
        EndTime = new DateTime(2020, 6, 1).AddMonths(1).AddMilliseconds(-1),
        UserId = _firstWorkerId,
        IsClosed = true,
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
      Assert.AreEqual(_firstLeaveTime, await _dbContext.LeaveTimes.FirstAsync());
    }

    [Test]
    public async Task CreateReturnNullAsync()
    {
      Assert.IsNull(await _repository.CreateAsync(null));
      Assert.AreEqual(0, await _dbContext.LeaveTimes.CountAsync());
    }

    [Test]
    public async Task EditSuccessfullyAsync()
    {
      await _dbContext.AddAsync(_editableLeaveTime);
      await _dbContext.SaveChangesAsync();

      string value = "EditedSickLeave";
      JsonPatchDocument<DbLeaveTime> request = new()
      {
        Operations = { new() { op = "replace", path = "/Comment", value = value } }
      };

      Assert.IsTrue(await _repository.EditAsync(_editableLeaveTime, request));
      Assert.AreEqual(value, (await _dbContext.LeaveTimes.FirstAsync()).Comment);
    }

    [Test]
    public async Task EditReturnFalseAsync()
    {
      Assert.IsFalse(await _repository.EditAsync(null, new JsonPatchDocument<DbLeaveTime>()));
    }

    [Test]
    public async Task FindByUserIdSuccessfullyAsync()
    {
      await _dbContext.AddRangeAsync(_firstLeaveTime, _secondLeaveTime, _thirdLeaveTime, _thirdManagerLeaveTime);
      await _dbContext.SaveChangesAsync();

      FindLeaveTimesFilter filter = new() { UserId = _firstLeaveTime.UserId, TakeCount = 2 };
      List<DbLeaveTime> expectedLeaveTimes = new() { _firstLeaveTime, _thirdLeaveTime };

      var leaveTimes = await _repository.FindAsync(filter);

      Assert.AreEqual(4, await _dbContext.LeaveTimes.CountAsync());
      CollectionAssert.AreEquivalent(expectedLeaveTimes, leaveTimes.Item1);
    }

    [Test]
    public async Task FindByStartTimeAndEndTimeSuccessfullyAsync()
    {
      await _dbContext.AddRangeAsync(_firstLeaveTime, _secondLeaveTime, _thirdLeaveTime, _thirdManagerLeaveTime);
      await _dbContext.SaveChangesAsync();

      FindLeaveTimesFilter filter = new()
      {
        StartTime = new DateTime(2020, 7, 1),
        EndTime = new DateTime(2021, 7, 1),
        TakeCount = int.MaxValue
      };

      List<DbLeaveTime> expectedLeaveTimes = new() { _firstLeaveTime, _secondLeaveTime, _thirdLeaveTime };

      Assert.AreEqual(4, await _dbContext.LeaveTimes.CountAsync());
      CollectionAssert.AreEquivalent(expectedLeaveTimes, (await _repository.FindAsync(filter)).Item1);
    }

    [Test]
    public async Task FindNothingAsync()
    {
      await _dbContext.AddRangeAsync(_firstLeaveTime, _secondLeaveTime, _thirdLeaveTime);
      await _dbContext.SaveChangesAsync();

      FindLeaveTimesFilter filter = new() { StartTime = new DateTime(2030, 12, 30) };
      (List<DbLeaveTime> leaveTimes, int totalCount) expectedLeaveTimes = new()
      {
        leaveTimes = new(),
        totalCount = 0
      };

      Assert.AreEqual(3, await _dbContext.LeaveTimes.CountAsync());
      Assert.AreEqual(expectedLeaveTimes, await _repository.FindAsync(filter));
    }

    [Test]
    public async Task GetByYearSuccessfullyAsync()
    {
      await _dbContext.AddRangeAsync(_thirdLeaveTime, _thirdManagerLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.AreEqual(
        new List<DbLeaveTime> { _thirdLeaveTime },
        await _repository.GetAsync(new List<Guid> { _thirdLeaveTime.UserId }, 2020, null, true));
    }

    [Test]
    public async Task GetByYearWithMonthSuccessfullyAsync()
    {
      await _dbContext.AddAsync(_firstLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.AreEqual(
        new List<DbLeaveTime> { _firstLeaveTime },
        await _repository.GetAsync(new List<Guid> { _firstLeaveTime.UserId }, 2020, 7));
    }

    [Test]
    public async Task GetByYearWithMonthNothingAsync()
    {
      await _dbContext.AddAsync(_firstLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsEmpty(await _repository.GetAsync(new List<Guid> { _firstLeaveTime.UserId }, 2020, 8));
    }

    [Test]
    public async Task GetByYearNothingAsync()
    {
      await _dbContext.AddRangeAsync(_thirdLeaveTime, _thirdManagerLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsEmpty(await _repository.GetAsync(
        new List<Guid> { _thirdLeaveTime.UserId },
        2022,
        null));
    }

    [Test]
    public async Task GetByYearNullWithoutUsersIdsAsync()
    {
      await _dbContext.AddAsync(_firstLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsNull(await _repository.GetAsync(null, 2020, null));
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

      Assert.IsNull(await _repository.GetAsync(_secondLeaveTime.Id));
    }

    [Test]
    public async Task HasOverlapByUserAsync()
    {
      await _dbContext.AddRangeAsync(_thirdLeaveTime, _thirdManagerLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsTrue(await _repository.HasOverlapAsync(
        _firstLeaveTime.UserId,
        _firstLeaveTime.StartTime,
        _firstLeaveTime.EndTime));
    }

    [Test]
    public async Task HasOverlapByUserWithProlongedInDatabaseAsync()
    {
      await _dbContext.AddRangeAsync(_openedProlongedLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsTrue(await _repository.HasOverlapAsync(
        _firstLeaveTime.UserId,
        _firstLeaveTime.StartTime,
        _firstLeaveTime.EndTime));
    }

    [Test]
    public async Task HasOverlapByUserWithNullEndTimeAsync()
    {
      await _dbContext.AddRangeAsync(_firstLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsTrue(await _repository.HasOverlapAsync(
        _openedProlongedLeaveTime.UserId,
        _openedProlongedLeaveTime.StartTime,
        null));
    }

    [Test]
    public async Task DoNotHaveOverlapByUserAsync()
    {
      await _dbContext.AddRangeAsync(_firstLeaveTime, _thirdLeaveTime, _closedProlongedLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsFalse(await _repository.HasOverlapAsync(
        _secondLeaveTime.UserId,
        _firstLeaveTime.StartTime,
        _firstLeaveTime.EndTime));
    }

    [Test]
    public async Task DoNotHaveOverlapByUserWithClosedProlongedAsync()
    {
      await _dbContext.AddRangeAsync(_secondLeaveTime, _closedProlongedLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsFalse(await _repository.HasOverlapAsync(
        _firstLeaveTime.UserId,
        _firstLeaveTime.StartTime,
        _firstLeaveTime.EndTime));
    }

    [Test]
    public async Task DoNotHaveOverlapByUserWithOpenedProlongedAsync()
    {
      await _dbContext.AddRangeAsync(_secondLeaveTime, _openedProlongedLeaveTime);
      await _dbContext.SaveChangesAsync();

      Assert.IsFalse(await _repository.HasOverlapAsync(
        _fourthLeaveTime.UserId,
        _fourthLeaveTime.StartTime,
        _fourthLeaveTime.EndTime));
    }

    [Test]
    public async Task HasOverlapByLeaveTimeAsync()
    {
      await _dbContext.AddRangeAsync(_thirdLeaveTime, _thirdManagerLeaveTime);
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
