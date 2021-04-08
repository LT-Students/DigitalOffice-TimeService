using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Enums;
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
                WorkerUserId = _firstWorkerId
            };
            _secondLeaveTime = new DbLeaveTime
            {
                Id = Guid.NewGuid(),
                LeaveType = (int)LeaveType.Training,
                Comment = "SickLeave",
                StartTime = new DateTime(2020, 7, 10),
                EndTime = new DateTime(2020, 7, 20),
                WorkerUserId = _secondWorkerId
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

        #region CreateLeaveTimeTests
        [Test]
        public void ShouldAddNewLeaveTimeInDb()
        {
            var guidOfNewLeaveTime = _repository.CreateLeaveTime(_firstLeaveTime);

            Assert.AreEqual(_firstLeaveTime.Id, guidOfNewLeaveTime);
            Assert.That(_dbContext.LeaveTimes.Find(_firstLeaveTime.Id), Is.EqualTo(_firstLeaveTime));
        }
        #endregion

        #region GetUserLeaveTimesTests
        [Test]
        public void ShouldReturnsLeaveTime()
        {
            _dbContext.Add(_firstLeaveTime);
            _dbContext.Add(_secondLeaveTime);
            _dbContext.SaveChanges();

            var leaveTimesOfFirstWorker = _repository.GetUserLeaveTimes(_firstWorkerId);
            var leaveTimesOfSecondWorker = _repository.GetUserLeaveTimes(_secondWorkerId);

            Assert.That(leaveTimesOfFirstWorker, Is.EquivalentTo(new[] {_firstLeaveTime}));
            Assert.That(leaveTimesOfSecondWorker, Is.EquivalentTo(new[] {_secondLeaveTime}));
            Assert.That(_dbContext.LeaveTimes, Is.EquivalentTo(new[] {_firstLeaveTime, _secondLeaveTime}));
        }
        #endregion
    }
}