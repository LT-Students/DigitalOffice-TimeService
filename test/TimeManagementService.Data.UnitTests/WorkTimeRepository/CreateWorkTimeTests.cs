using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Data.UnitTests
{
    public class CreateWorkTimeTests
    {
        private TimeManagementDbContext dbContext;
        private IWorkTimeRepository repository;

        private DbWorkTime dbWorkTime;

        [SetUp]
        public void OneTimeSetUp()
        {
            var dbOptions = new DbContextOptionsBuilder<TimeManagementDbContext>()
                                    .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                                    .Options;

            dbContext = new TimeManagementDbContext(dbOptions);
            repository = new WorkTimeRepository(dbContext);

            dbWorkTime = new DbWorkTime
            {
                Id = Guid.NewGuid(),
                Title = $"WorkTime",
                UserId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(-0.75)
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
        public void SuccessfulAddNewWorkTimeInDb()
        {
            var guidOfNewWorkTime = repository.CreateWorkTime(dbWorkTime);

            Assert.AreEqual(dbWorkTime.Id, guidOfNewWorkTime);
            Assert.NotNull(dbContext.WorkTimes.Find(dbWorkTime.Id));
        }
    }
}