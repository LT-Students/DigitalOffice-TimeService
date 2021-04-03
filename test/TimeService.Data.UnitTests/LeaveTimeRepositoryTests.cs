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
        private TimeServiceDbContext dbContext;
        private ILeaveTimeRepository repository;

        private Guid firstWorkerId;
        private Guid secondWorkerId;
        private DbLeaveTime firstLeaveTime;
        private DbLeaveTime secondLeaveTime;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var dbOptions = new DbContextOptionsBuilder<TimeServiceDbContext>()
                                    .UseInMemoryDatabase("InMemoryDatabase")
                                    .Options;
            dbContext = new TimeServiceDbContext(dbOptions);
            repository = new LeaveTimeRepository(dbContext);

            firstWorkerId = Guid.NewGuid();
            secondWorkerId = Guid.NewGuid();

            firstLeaveTime = new DbLeaveTime
            {
                Id = Guid.NewGuid(),
                LeaveType = (int)LeaveType.SickLeave,
                Comment = "SickLeave",
                StartTime = new DateTime(2020, 7, 5),
                EndTime = new DateTime(2020, 7, 25),
                WorkerUserId = firstWorkerId
            };
            secondLeaveTime = new DbLeaveTime
            {
                Id = Guid.NewGuid(),
                LeaveType = (int)LeaveType.Training,
                Comment = "SickLeave",
                StartTime = new DateTime(2020, 7, 10),
                EndTime = new DateTime(2020, 7, 20),
                WorkerUserId = secondWorkerId
            };
        }

        [TearDown]
        public void CleanDb()
        {
            if (dbContext.Database.IsInMemory())
            {
                dbContext.Database.EnsureDeleted();
            }
        }

        #region CreateLeaveTimeTests
        [Test]
        public void ShouldAddNewLeaveTimeInDb()
        {
            var guidOfNewLeaveTime = repository.CreateLeaveTime(firstLeaveTime);

            Assert.AreEqual(firstLeaveTime.Id, guidOfNewLeaveTime);
            Assert.That(dbContext.LeaveTimes.Find(firstLeaveTime.Id), Is.EqualTo(firstLeaveTime));
        }
        #endregion

        #region GetUserLeaveTimesTests
        [Test]
        public void ShouldReturnsLeaveTime()
        {
            dbContext.Add(firstLeaveTime);
            dbContext.Add(secondLeaveTime);
            dbContext.SaveChanges();

            var leaveTimesOfFirstWorker = repository.GetUserLeaveTimes(firstWorkerId);
            var leaveTimesOfSecondWorker = repository.GetUserLeaveTimes(secondWorkerId);

            Assert.That(leaveTimesOfFirstWorker, Is.EquivalentTo(new[] {firstLeaveTime}));
            Assert.That(leaveTimesOfSecondWorker, Is.EquivalentTo(new[] {secondLeaveTime}));
            Assert.That(dbContext.LeaveTimes, Is.EquivalentTo(new[] {firstLeaveTime, secondLeaveTime}));
        }
        #endregion
    }
}