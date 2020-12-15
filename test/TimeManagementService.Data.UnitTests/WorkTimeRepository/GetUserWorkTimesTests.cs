using LT.DigitalOffice.TimeManagementService.Data.Filters;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeManagementService.Data.UnitTests
{
    public class GetUserWorkTimesTests
    {
        private TimeManagementDbContext dbContext;
        private IWorkTimeRepository repository;

        private Guid project1;
        private Guid project2;
        private Guid worker1;
        private Guid worker2;
        private List<DbWorkTime> workTimesOfWorker1;
        private List<DbWorkTime> workTimesOfWorker2;

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
    }
}