using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Provider;
using LT.DigitalOffice.TimeManagementService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Enums;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Data.UnitTests.LeaveTimeRepositoryTests
{
    public class CreateLeaveTimeTests
    {
        private IDataProvider provider;
        private ILeaveTimeRepository repository;

        private DbLeaveTime leaveTime;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var dbOptions = new DbContextOptionsBuilder<TimeManagementDbContext>()
                                    .UseInMemoryDatabase("InMemoryDatabase")
                                    .Options;
            provider = new TimeManagementDbContext(dbOptions);
            repository = new LeaveTimeRepository(provider);

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
            if (provider.IsInMemory())
            {
                provider.EnsureDeleted();
            }
        }

        [Test]
        public void ShouldAddNewLeaveTimeInDb()
        {
            var guidOfNewLeaveTime = repository.CreateLeaveTime(leaveTime);

            Assert.AreEqual(leaveTime.Id, guidOfNewLeaveTime);
            Assert.That(provider.LeaveTimes.Find(leaveTime.Id), Is.EqualTo(leaveTime));
        }
    }
}