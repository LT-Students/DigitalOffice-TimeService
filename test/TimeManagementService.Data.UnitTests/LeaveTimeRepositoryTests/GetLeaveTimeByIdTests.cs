using LT.DigitalOffice.Kernel.Exceptions;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Provider;
using LT.DigitalOffice.TimeManagementService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Data.UnitTests.LeaveTimeRepositoryTests
{
    public class GetLeaveTimeByIdTests
    {
        private IDataProvider provider;
        private ILeaveTimeRepository repository;

        private Guid dbLeaveTimeId;
        private DbLeaveTime dbLeaveTime;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var dbOptions = new DbContextOptionsBuilder<TimeManagementDbContext>()
                                    .UseInMemoryDatabase("InMemoryDatabase")
                                    .Options;
            provider = new TimeManagementDbContext(dbOptions);
            repository = new LeaveTimeRepository(provider);

            dbLeaveTimeId = Guid.NewGuid();

            dbLeaveTime = new DbLeaveTime
            {
                Id = dbLeaveTimeId,
                UserId = Guid.NewGuid()
            };

            provider.LeaveTimes.Add(dbLeaveTime);
            provider.Save();
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
        public void ShouldReturnsLeaveTimeWhenLeaveTimeWithThisIdIsInDb()
        {
            Assert.AreEqual(dbLeaveTime, repository.GetLeaveTimeById(dbLeaveTimeId));
        }

        [Test]
        public void ShouldThrowlExceptionWhenLeaveTimeWithThisIdNotExistInDb()
        {
            Assert.Throws<NotFoundException>(() => repository.GetLeaveTimeById(Guid.NewGuid()));
        }
    }
}