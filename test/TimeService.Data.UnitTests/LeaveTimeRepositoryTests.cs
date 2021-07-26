using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;

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
                UserId = _firstWorkerId
            };
            _secondLeaveTime = new DbLeaveTime
            {
                Id = Guid.NewGuid(),
                LeaveType = (int)LeaveType.Training,
                Comment = "SickLeave",
                StartTime = new DateTime(2020, 7, 10),
                EndTime = new DateTime(2020, 7, 20),
                UserId = _secondWorkerId
            };
            _thirdLeaveTime = new DbLeaveTime
            {
                Id = Guid.NewGuid(),
                LeaveType = (int)LeaveType.SickLeave,
                Comment = "SickLeave",
                StartTime = new DateTime(2020, 7, 5),
                EndTime = new DateTime(2020, 7, 25),
                UserId = _firstWorkerId
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

        #region Create

        [Test]
        public void ShouldAddNewLeaveTimeInDb()
        {
            var guidOfNewLeaveTime = _repository.Add(_firstLeaveTime);

            Assert.AreEqual(_firstLeaveTime.Id, guidOfNewLeaveTime);
            Assert.That(_dbContext.LeaveTimes.Find(_firstLeaveTime.Id), Is.EqualTo(_firstLeaveTime));
        }

        #endregion

        #region Find

        [Test]
        public void ShouldSuccessfulFindAll()
        {
            _dbContext.Add(_firstLeaveTime);
            _dbContext.Add(_secondLeaveTime);
            _dbContext.Add(_thirdLeaveTime);
            _dbContext.SaveChanges();

            int count;

            _repository.Find(
                new FindLeaveTimesFilter { },
                0,
                int.MaxValue,
                out count);

            Assert.AreEqual(3, count);
        }

        [Test]
        public void ShouldSuccessfulFindNothing()
        {
            _dbContext.Add(_firstLeaveTime);
            _dbContext.Add(_secondLeaveTime);
            _dbContext.Add(_thirdLeaveTime);
            _dbContext.SaveChanges();

            int count;

            _repository.Find(
                new FindLeaveTimesFilter
                {
                    UserId = Guid.NewGuid()
                },
                0,
                int.MaxValue,
                out count);

            Assert.AreEqual(0, count);

            _repository.Find(
                new FindLeaveTimesFilter
                {
                    StartTime = DateTime.Now.AddDays(10000),
                    EndTime = DateTime.Now.AddDays(-10000)
                },
                0,
                int.MaxValue,
                out count);

            Assert.AreEqual(0, count);
        }

        [Test]
        public void ShouldReturnCorrectlyCount()
        {
            _dbContext.Add(_firstLeaveTime);
            _dbContext.Add(_secondLeaveTime);
            _dbContext.Add(_thirdLeaveTime);
            _dbContext.SaveChanges();

            int count;

            _repository.Find(
                new FindLeaveTimesFilter
                {
                    UserId = _firstWorkerId,
                    StartTime = DateTime.Now.AddDays(-100000),
                    EndTime = DateTime.Now.AddDays(100000)
                },
                0,
                int.MaxValue,
                out count);

            Assert.AreEqual(2, count);

            _repository.Find(
                new FindLeaveTimesFilter
                {
                    UserId = _secondWorkerId,
                    StartTime = DateTime.Now.AddDays(-100000),
                    EndTime = DateTime.Now.AddDays(100000)
                },
                0,
                int.MaxValue,
                out count);

            Assert.AreEqual(1, count);
        }

        #endregion
    }
}