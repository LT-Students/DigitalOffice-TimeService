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

namespace LT.DigitalOffice.TimeManagementService.Data.UnitTests.WorkTimeRepositoryTests
{
    public class EditWorkTimeTests
    {
        private IDataProvider provider;
        private IWorkTimeRepository repository;

        private DbWorkTime dbWorkTime;
        private DbWorkTime newDbWorkTime;
        private Guid workTimeId;

        [SetUp]
        public void SetUp()
        {
            var dbOptions = new DbContextOptionsBuilder<TimeManagementDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                .Options;

            provider = new TimeManagementDbContext(dbOptions);
            repository = new WorkTimeRepository(provider);

            workTimeId = Guid.NewGuid();

            dbWorkTime = new DbWorkTime
            {
                Id = workTimeId,
                UserId = Guid.NewGuid(),
                StartDate = new DateTime(2020, 1, 1, 1, 1, 1),
                EndDate = new DateTime(2020, 2, 2, 2, 2, 2),
                Title = "Example",
                ProjectId = Guid.NewGuid(),
                Description = "ExampleDescription"
            };

            newDbWorkTime = new DbWorkTime
            {
                Id = workTimeId,
                UserId = Guid.NewGuid(),
                StartDate = new DateTime(2020, 1, 1, 1, 1, 1),
                EndDate = new DateTime(2020, 2, 2, 2, 2, 2),
                Title = "ExampleTitle",
                ProjectId = Guid.NewGuid(),
                Description = "Example"
            };

            provider.WorkTimes.Add(dbWorkTime);
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
        public void ShouldChangeWorkTimeWhenDataIsCorrect()
        {
            Assert.True(repository.EditWorkTime(newDbWorkTime));

            var result = provider.WorkTimes.AsNoTracking().First(x => x.Id == workTimeId);

            SerializerAssert.AreEqual(newDbWorkTime, result);
        }

        [Test]
        public void ShouldThrowExceptionWhenIdIsNotExist()
        {
            newDbWorkTime.Id = Guid.NewGuid();

            Assert.Throws<NotFoundException>(() => repository.EditWorkTime(newDbWorkTime));
        }
    }
}