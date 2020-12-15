using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Enums;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Data.UnitTests
{
    public class CreateLeaveTimeTests
    {
        private TimeManagementDbContext dbContext;
        private ILeaveTimeRepository repository;

        private Guid firstWorkerId;
        private Guid secondWorkerId;
        private DbLeaveTime firstLeaveTime;
        private DbLeaveTime secondLeaveTime;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var dbOptions = new DbContextOptionsBuilder<TimeManagementDbContext>()
                                    .UseInMemoryDatabase("InMemoryDatabase")
                                    .Options;
            dbContext = new TimeManagementDbContext(dbOptions);
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
                UserId = firstWorkerId
            };
            secondLeaveTime = new DbLeaveTime
            {
                Id = Guid.NewGuid(),
                LeaveType = (int)LeaveType.Training,
                Comment = "SickLeave",
                StartTime = new DateTime(2020, 7, 10),
                EndTime = new DateTime(2020, 7, 20),
                UserId = secondWorkerId
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

        [Test]
        public void ShouldAddNewLeaveTimeInDb()
        {
            var guidOfNewLeaveTime = repository.CreateLeaveTime(firstLeaveTime);

            Assert.AreEqual(firstLeaveTime.Id, guidOfNewLeaveTime);
            Assert.That(dbContext.LeaveTimes.Find(firstLeaveTime.Id), Is.EqualTo(firstLeaveTime));
        }
    }
}