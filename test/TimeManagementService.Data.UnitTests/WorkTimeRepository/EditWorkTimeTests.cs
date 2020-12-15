using LT.DigitalOffice.Kernel.Exceptions;
using LT.DigitalOffice.TimeManagementService.Data.Filters;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.UnitTestKernel;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeManagementService.Data.UnitTests
{
    public class EditWorkTimeTests
    {
        private TimeManagementDbContext dbContext;
        private IWorkTimeRepository repository;

        private Guid project1;
        private Guid project2;
        private Guid worker1;
        private Guid worker2;
        private List<DbWorkTime> workTimesOfWorker1;
        private List<DbWorkTime> workTimesOfWorker2;

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

            project1 = Guid.NewGuid();
            project2 = Guid.NewGuid();
            worker1 = Guid.NewGuid();
            worker2 = Guid.NewGuid();

            workTimesOfWorker1 = new List<DbWorkTime>
            {
                new DbWorkTime
                {
                    Id = Guid.NewGuid(),
                    Title = $"WorkTime",
                    UserId = worker1,
                    ProjectId = project1,
                    StartDate = DateTime.Now.AddDays(-1),
                    EndDate = DateTime.Now.AddDays(-0.75)
                },

                new DbWorkTime
                {
                    Id = Guid.NewGuid(),
                    Title = $"WorkTime",
                    UserId = worker1,
                    ProjectId = project2,
                    StartDate = DateTime.Now.AddDays(-0.7),
                    EndDate = DateTime.Now.AddDays(-0.45)
                }
            };

            workTimesOfWorker2 = new List<DbWorkTime>
            {
                new DbWorkTime
                {
                    Id = Guid.NewGuid(),
                    Title = $"WorkTime",
                    UserId = worker2,
                    ProjectId = project1,
                    StartDate = DateTime.Now.AddDays(-0.9),
                    EndDate = DateTime.Now.AddDays(-0.65)
                }
            };

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

        [Test]
        public void ShouldChangeWorkTimeWhenDataIsCorrect()
        {
            var newTime = new DbWorkTime
            {
                Id = id,
                UserId = Guid.NewGuid(),
                StartDate = new DateTime(2020, 1, 1, 1, 1, 1),
                EndDate = new DateTime(2020, 2, 2, 2, 2, 2),
                Title = "ExampleTitle",
                ProjectId = Guid.NewGuid(),
                Description = "Example"
            };

            dbContext.WorkTimes.Add(time);
            dbContext.SaveChanges();

            Assert.True(repository.EditWorkTime(newTime));

            var result = dbContext.WorkTimes.Find(id);

            SerializerAssert.AreEqual(newTime, result);
        }

        [Test]
        public void ShouldThrowExceptionWhenIdIsNotExist()
        {
            Assert.Throws<NotFoundException>(() => repository.EditWorkTime(time));
        }
    }
}