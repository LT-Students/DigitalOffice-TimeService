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
    public class GetUserLeaveTimesTests
    {
        private IDataProvider provider;
        private ILeaveTimeRepository repository;

        private Guid userId1;
        private Guid userId2;
        private DbLeaveTime leaveTimeOfUser1;
        private DbLeaveTime leaveTimeOfUser2;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var dbOptions = new DbContextOptionsBuilder<TimeManagementDbContext>()
                                    .UseInMemoryDatabase("InMemoryDatabase")
                                    .Options;
            provider = new TimeManagementDbContext(dbOptions);
            repository = new LeaveTimeRepository(provider);

            userId1 = Guid.NewGuid();
            userId2 = Guid.NewGuid();

            leaveTimeOfUser1 = new DbLeaveTime
            {
                Id = Guid.NewGuid(),
                LeaveType = (int)LeaveType.SickLeave,
                Comment = "SickLeave",
                StartTime = new DateTime(2020, 7, 5),
                EndTime = new DateTime(2020, 7, 25),
                UserId = userId1
            };
            leaveTimeOfUser2 = new DbLeaveTime
            {
                Id = Guid.NewGuid(),
                LeaveType = (int)LeaveType.Training,
                Comment = "SickLeave",
                StartTime = new DateTime(2020, 7, 10),
                EndTime = new DateTime(2020, 7, 20),
                UserId = userId2
            };

            provider.LeaveTimes.AddRange(leaveTimeOfUser1, leaveTimeOfUser2);
            provider.Save();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (provider.IsInMemory())
            {
                provider.EnsureDeleted();
            }
        }

        [Test]
        public void ShouldReturnsLeaveTimeWhenIdIsInDb()
        {
            var leaveTimesOfUser1 = repository.GetUserLeaveTimes(userId1);
            var leaveTimesOfUser2 = repository.GetUserLeaveTimes(userId2);

            Assert.That(leaveTimesOfUser1, Is.EquivalentTo(new[] { leaveTimeOfUser1 }));
            Assert.That(leaveTimesOfUser2, Is.EquivalentTo(new[] { leaveTimeOfUser2 }));
            Assert.That(provider.LeaveTimes, Is.EquivalentTo(new[] { leaveTimeOfUser1, leaveTimeOfUser2 }));
        }

        [Test]
        public void ShouldReturnsEmptyListWhenIdIsNotInDb()
        {
            var leaveTimes = repository.GetUserLeaveTimes(Guid.NewGuid());

            Assert.That(leaveTimes, Is.EquivalentTo(Array.Empty<DbLeaveTime>()));

            Assert.That(provider.LeaveTimes, Is.EquivalentTo(new[] { leaveTimeOfUser1, leaveTimeOfUser2 }));
        }
    }
}