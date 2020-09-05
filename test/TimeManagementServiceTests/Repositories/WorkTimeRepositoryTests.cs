using LT.DigitalOffice.TimeManagementService.Database;
using LT.DigitalOffice.TimeManagementService.Database.Entities;
using LT.DigitalOffice.TimeManagementService.Repositories;
using LT.DigitalOffice.TimeManagementService.Repositories.Filters;
using LT.DigitalOffice.TimeManagementService.Repositories.Interfaces;
using LT.DigitalOffice.TimeManagementServiceUnitTests.UnitTestLibrary;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeManagementServiceUnitTests.Repositories
{
    public class CreateWorkTimeTests
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
                    WorkerUserId = worker1,
                    ProjectId = project1,
                    StartTime = DateTime.Now.AddDays(-1),
                    EndTime = DateTime.Now.AddDays(-0.75)
                },

                new DbWorkTime
                {
                    Id = Guid.NewGuid(),
                    Title = $"WorkTime",
                    WorkerUserId = worker1,
                    ProjectId = project2,
                    StartTime = DateTime.Now.AddDays(-0.7),
                    EndTime = DateTime.Now.AddDays(-0.45)
                }
            };

            workTimesOfWorker2 = new List<DbWorkTime>
            {
                new DbWorkTime
                {
                    Id = Guid.NewGuid(),
                    Title = $"WorkTime",
                    WorkerUserId = worker2,
                    ProjectId = project1,
                    StartTime = DateTime.Now.AddDays(-0.9),
                    EndTime = DateTime.Now.AddDays(-0.65)
                }
            };

            id = Guid.NewGuid();

            time = new DbWorkTime
            {
                Id = id,
                WorkerUserId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 1, 1, 1, 1, 1),
                EndTime = new DateTime(2020, 2, 2, 2, 2, 2),
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

        #region CreateWorkTimeTests
        [Test]
        public void SuccessfulAddNewWorkTimeInDb()
        {
            var guidOfNewWorkTime = repository.CreateWorkTime(workTimesOfWorker1.First());

            Assert.AreEqual(workTimesOfWorker1.First().Id, guidOfNewWorkTime);
            Assert.NotNull(dbContext.WorkTimes.Find(workTimesOfWorker1.First().Id));
        }
        #endregion

        #region GetUserWorkTimesTests
        [Test]
        public void CorrectlyReturnsWorkTime()
        {
            foreach (var wt in workTimesOfWorker1)
            {
                dbContext.Add(wt);
            }

            foreach (var wt in workTimesOfWorker2)
            {
                dbContext.Add(wt);
            }
            dbContext.SaveChanges();

            var filterForGetNothing = new WorkTimeFilter
            {
                StartTime = DateTime.Now.AddDays(-10),
                EndTime = DateTime.Now.AddDays(-5)
            };

            var filterForGetEverything = new WorkTimeFilter
            {
                StartTime = DateTime.Now.AddDays(-10),
                EndTime = DateTime.Now.AddDays(10)
            };

            var filerForGetOnlyWorkTime2 = new WorkTimeFilter
            {
                StartTime = DateTime.Now.AddDays(-0.95),
                EndTime = DateTime.Now.AddDays(-0.6)
            };

            Assert.AreEqual(
                repository.GetUserWorkTimes(worker1, filterForGetEverything).Count,
                workTimesOfWorker1.Count);

            Assert.AreEqual(
                repository.GetUserWorkTimes(worker2, filterForGetEverything).Count,
                workTimesOfWorker2.Count);

            Assert.AreEqual(repository.GetUserWorkTimes(worker1, filterForGetNothing).Count, 0);

            Assert.AreEqual(repository.GetUserWorkTimes(worker2, filterForGetNothing).Count, 0);

            Assert.AreEqual(repository.GetUserWorkTimes(worker1, filerForGetOnlyWorkTime2).Count, 0);

            Assert.AreEqual(
                repository.GetUserWorkTimes(worker2, filerForGetOnlyWorkTime2).Count,
                workTimesOfWorker2.Count);
        }
        #endregion

        #region EditWorkTimeTests
        [Test]
        public void ShouldChangeWorkTimeWhenDataIsCorrect()
        {
            var newTime = new DbWorkTime
            {
                Id = id,
                WorkerUserId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 1, 1, 1, 1, 1),
                EndTime = new DateTime(2020, 2, 2, 2, 2, 2),
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
            Assert.Throws<Exception>(() => repository.EditWorkTime(time));
        }
        #endregion
    }
}