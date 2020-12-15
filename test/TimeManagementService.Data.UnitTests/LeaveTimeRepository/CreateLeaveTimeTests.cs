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

        private DbLeaveTime leaveTime;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var dbOptions = new DbContextOptionsBuilder<TimeManagementDbContext>()
                                    .UseInMemoryDatabase("InMemoryDatabase")
                                    .Options;
            dbContext = new TimeManagementDbContext(dbOptions);
            repository = new LeaveTimeRepository(dbContext);

            leaveTime = new DbLeaveTime
            {
                Id = Guid.NewGuid(),
                LeaveType = (int)LeaveType.SickLeave,
                Comment = "SickLeave",
                StartTime = new DateTime(2020, 7, 5),
                EndTime = new DateTime(2020, 7, 25),
                UserId = Guid.NewGuid()
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
            var guidOfNewLeaveTime = repository.CreateLeaveTime(leaveTime);

            Assert.AreEqual(leaveTime.Id, guidOfNewLeaveTime);
            Assert.That(dbContext.LeaveTimes.Find(leaveTime.Id), Is.EqualTo(leaveTime));
        }
    }
}