using LT.DigitalOffice.Kernel.Exceptions;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.UnitTestKernel;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Data.UnitTests
{
    public class EditWorkTimeTests
    {
        private TimeManagementDbContext dbContext;
        private IWorkTimeRepository repository;

        private DbWorkTime time;
        private Guid id;

        [SetUp]
        public void OneTimeSetUp()
        {
            var dbOptions = new DbContextOptionsBuilder<TimeManagementDbContext>()
                                    .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                                    .Options;

            dbContext = new TimeManagementDbContext(dbOptions);
            repository = new WorkTimeRepository(dbContext);

            id = Guid.NewGuid();

            time = new DbWorkTime
            {
                Id = id,
                UserId = Guid.NewGuid(),
                StartDate = new DateTime(2020, 1, 1, 1, 1, 1),
                EndDate = new DateTime(2020, 2, 2, 2, 2, 2),
                Title = "Example",
                ProjectId = Guid.NewGuid(),
                Description = "ExampleDescription"
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

        //[Test]
        //public void ShouldChangeWorkTimeWhenDataIsCorrect()
        //{
        //    var newTime = new DbWorkTime
        //    {
        //        Id = id,
        //        UserId = Guid.NewGuid(),
        //        StartDate = new DateTime(2020, 1, 1, 1, 1, 1),
        //        EndDate = new DateTime(2020, 2, 2, 2, 2, 2),
        //        Title = "ExampleTitle",
        //        ProjectId = Guid.NewGuid(),
        //        Description = "Example"
        //    };

        //    dbContext.WorkTimes.Add(time);
        //    dbContext.SaveChanges();

        //    Assert.True(repository.EditWorkTime(newTime));

        //    var result = dbContext.WorkTimes.Find(id);

        //    SerializerAssert.AreEqual(newTime, result);
        //}

        [Test]
        public void ShouldThrowExceptionWhenIdIsNotExist()
        {
            Assert.Throws<NotFoundException>(() => repository.EditWorkTime(time));
        }
    }
}