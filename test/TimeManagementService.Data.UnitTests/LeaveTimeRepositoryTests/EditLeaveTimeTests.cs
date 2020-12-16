using LT.DigitalOffice.Kernel.Exceptions;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Provider;
using LT.DigitalOffice.TimeManagementService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.UnitTestKernel;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Linq;

namespace LT.DigitalOffice.TimeManagementService.Data.UnitTests.LeaveTimeRepositoryTests
{
    public class EditLeaveTimeTests
    {
        private IDataProvider provider;
        private ILeaveTimeRepository repository;

        private DbLeaveTime dbLeaveTime;
        private DbLeaveTime newDbLeaveTime;
        private Guid leaveTimeId;

        [SetUp]
        public void SetUp()
        {
            var dbOptions = new DbContextOptionsBuilder<TimeManagementDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                .Options;

            provider = new TimeManagementDbContext(dbOptions);
            repository = new LeaveTimeRepository(provider);

            leaveTimeId = Guid.NewGuid();

            dbLeaveTime = new DbLeaveTime
            {
                Id = leaveTimeId,
                UserId = Guid.NewGuid()
            };

            newDbLeaveTime = new DbLeaveTime
            {
                Id = leaveTimeId,
                UserId = Guid.NewGuid()
            };

            provider.LeaveTimes.Add(dbLeaveTime);
            provider.Save();
        }

        [TearDown]
        public void TearDown()
        {
            if (provider.IsInMemory())
            {
                provider.EnsureDeleted();
            }
        }

        [Test]
        public void ShouldChangeLeaveTimeWhenDataIsCorrect()
        {
            Assert.True(repository.EditLeaveTime(newDbLeaveTime));

            var result = provider.WorkTimes.AsNoTracking().First(x => x.Id == leaveTimeId);

            SerializerAssert.AreEqual(newDbLeaveTime, result);
        }

        [Test]
        public void ShouldThrowExceptionWhenIdIsNotExist()
        {
            newDbLeaveTime.Id = Guid.NewGuid();

            Assert.Throws<NotFoundException>(() => repository.EditLeaveTime(newDbLeaveTime));
        }
    }
}