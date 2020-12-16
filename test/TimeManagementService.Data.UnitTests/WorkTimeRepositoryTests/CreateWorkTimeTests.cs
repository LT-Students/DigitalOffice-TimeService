using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Provider;
using LT.DigitalOffice.TimeManagementService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Data.UnitTests.WorkTimeRepositoryTests
{
    public class CreateWorkTimeTests
    {
        private IDataProvider provider;
        private IWorkTimeRepository repository;

        private DbWorkTime dbWorkTime;

        [SetUp]
        public void OneTimeSetUp()
        {
            var dbOptions = new DbContextOptionsBuilder<TimeManagementDbContext>()
                                    .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                                    .Options;

            provider = new TimeManagementDbContext(dbOptions);
            repository = new WorkTimeRepository(provider);

            dbWorkTime = new DbWorkTime
            {
                Id = Guid.NewGuid(),
                Title = "WorkTime",
                UserId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(-0.75)
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
        public void SuccessfulAddNewWorkTimeInDb()
        {
            var guidOfNewWorkTime = repository.CreateWorkTime(dbWorkTime);

            Assert.AreEqual(dbWorkTime.Id, guidOfNewWorkTime);
            Assert.NotNull(provider.WorkTimes.Find(dbWorkTime.Id));
        }
    }
}