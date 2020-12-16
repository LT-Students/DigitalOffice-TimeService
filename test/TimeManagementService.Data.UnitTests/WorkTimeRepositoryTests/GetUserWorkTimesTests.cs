using LT.DigitalOffice.TimeManagementService.Data.Filters;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Provider;
using LT.DigitalOffice.TimeManagementService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeManagementService.Data.UnitTests.WorkTimeRepositoryTests
{
    public class GetUserWorkTimesTests
    {
        private IDataProvider provider;
        private IWorkTimeRepository repository;

        private Guid userId1;
        private Guid userId2;
        private List<DbWorkTime> workTimesOfUser1;
        private List<DbWorkTime> workTimesOfUser2;

        [SetUp]
        public void OneTimeSetUp()
        {
            var dbOptions = new DbContextOptionsBuilder<TimeManagementDbContext>()
                                    .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                                    .Options;

            provider = new TimeManagementDbContext(dbOptions);
            repository = new WorkTimeRepository(provider);

            var project1 = Guid.NewGuid();
            var project2 = Guid.NewGuid();
            userId1 = Guid.NewGuid();
            userId2 = Guid.NewGuid();

            workTimesOfUser1 = new List<DbWorkTime>
            {
                new DbWorkTime
                {
                    Id = Guid.NewGuid(),
                    Title = $"WorkTime",
                    UserId = userId1,
                    ProjectId = project1,
                    StartDate = DateTime.Now.AddDays(-1),
                    EndDate = DateTime.Now.AddDays(-0.75)
                },

                new DbWorkTime
                {
                    Id = Guid.NewGuid(),
                    Title = $"WorkTime",
                    UserId = userId1,
                    ProjectId = project2,
                    StartDate = DateTime.Now.AddDays(-0.7),
                    EndDate = DateTime.Now.AddDays(-0.45)
                }
            };

            workTimesOfUser2 = new List<DbWorkTime>
            {
                new DbWorkTime
                {
                    Id = Guid.NewGuid(),
                    Title = $"WorkTime",
                    UserId = userId2,
                    ProjectId = project1,
                    StartDate = DateTime.Now.AddDays(-0.9),
                    EndDate = DateTime.Now.AddDays(-0.65)
                }
            };

            foreach (var wt in workTimesOfUser1)
            {
                provider.WorkTimes.Add(wt);
            }

            foreach (var wt in workTimesOfUser2)
            {
                provider.WorkTimes.Add(wt);
            }
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
        public void ShouldReturnAllUserWorkTimesWhenFilterIsNull()
        {
            Assert.AreEqual(
                repository.GetUserWorkTimes(userId1, null).Count,
                workTimesOfUser1.Count);
        }

        [Test]
        public void ShouldReturnOnlyWorkTimesOfUser2WhenFilterForGetOnlyWorkTimesOfUser2()
        {
            var filerForGetOnlyWorkTimesOfUser2 = new WorkTimeFilter
            {
                StartTime = DateTime.Now.AddDays(-0.95),
                EndTime = DateTime.Now.AddDays(-0.6)
            };

            Assert.AreEqual(repository.GetUserWorkTimes(userId1, filerForGetOnlyWorkTimesOfUser2).Count, 0);

            Assert.AreEqual(
                repository.GetUserWorkTimes(userId2, filerForGetOnlyWorkTimesOfUser2).Count,
                workTimesOfUser2.Count);
        }

        [Test]
        public void ShouldReturnEverythingWhenFilterForGetEverything()
        {
            var filterForGetEverything = new WorkTimeFilter
            {
                StartTime = DateTime.Now.AddDays(-10),
                EndTime = DateTime.Now.AddDays(10)
            };

            Assert.AreEqual(
                repository.GetUserWorkTimes(userId1, filterForGetEverything).Count,
                workTimesOfUser1.Count);

            Assert.AreEqual(
                repository.GetUserWorkTimes(userId2, filterForGetEverything).Count,
                workTimesOfUser2.Count);
        }

        [Test]
        public void ShouldReturnNothingWhenFilterForGetNothing()
        {
            var filterForGetNothing = new WorkTimeFilter
            {
                StartTime = DateTime.Now.AddDays(-10),
                EndTime = DateTime.Now.AddDays(-5)
            };

            Assert.AreEqual(repository.GetUserWorkTimes(userId1, filterForGetNothing).Count, 0);

            Assert.AreEqual(repository.GetUserWorkTimes(userId2, filterForGetNothing).Count, 0);
        }
    }
}