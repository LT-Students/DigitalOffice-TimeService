using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.UnitTestKernel;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Data.UnitTests
{
    public class CreateWorkTimeTests
    {
        private TimeServiceDbContext _dbContext;
        private IWorkTimeRepository _repository;

        private Guid _project1;
        private Guid _project2;
        private Guid _worker1;
        private Guid _worker2;
        private List<DbWorkTime> _workTimesOfWorker1;
        private List<DbWorkTime> _workTimesOfWorker2;

        private DbWorkTime _time;
        private Guid _id;

        [SetUp]
        public void OneTimeSetUp()
        {
            var dbOptions = new DbContextOptionsBuilder<TimeServiceDbContext>()
                                    .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                                    .Options;

            _dbContext = new TimeServiceDbContext(dbOptions);
            _repository = new WorkTimeRepository(_dbContext);

            _project1 = Guid.NewGuid();
            _project2 = Guid.NewGuid();
            _worker1 = Guid.NewGuid();
            _worker2 = Guid.NewGuid();

            _workTimesOfWorker1 = new List<DbWorkTime>
            {
                new DbWorkTime
                {
                    Id = Guid.NewGuid(),
                    Title = $"WorkTime",
                    UserId = _worker1,
                    ProjectId = _project1,
                    StartTime = DateTime.Now.AddDays(-1),
                    EndTime = DateTime.Now.AddDays(-0.75)
                },

                new DbWorkTime
                {
                    Id = Guid.NewGuid(),
                    Title = $"WorkTime",
                    UserId = _worker1,
                    ProjectId = _project2,
                    StartTime = DateTime.Now.AddDays(-0.7),
                    EndTime = DateTime.Now.AddDays(-0.45)
                }
            };

            _workTimesOfWorker2 = new List<DbWorkTime>
            {
                new DbWorkTime
                {
                    Id = Guid.NewGuid(),
                    Title = $"WorkTime",
                    UserId = _worker2,
                    ProjectId = _project1,
                    StartTime = DateTime.Now.AddDays(-0.9),
                    EndTime = DateTime.Now.AddDays(-0.65)
                }
            };

            _id = Guid.NewGuid();

            _time = new DbWorkTime
            {
                Id = _id,
                UserId = Guid.NewGuid(),
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
            if (_dbContext.Database.IsInMemory())
            {
                _dbContext.Database.EnsureDeleted();
            }
        }

        #region CreateWorkTimeTests
        [Test]
        public void SuccessfulAddNewWorkTimeInDb()
        {
            var guidOfNewWorkTime = _repository.CreateWorkTime(_workTimesOfWorker1.First());

            Assert.AreEqual(_workTimesOfWorker1.First().Id, guidOfNewWorkTime);
            Assert.NotNull(_dbContext.WorkTimes.Find(_workTimesOfWorker1.First().Id));
        }
        #endregion

        #region GetUserWorkTimesTests
        [Test]
        public void CorrectlyReturnsWorkTime()
        {
            foreach (var wt in _workTimesOfWorker1)
            {
                _dbContext.Add(wt);
            }

            foreach (var wt in _workTimesOfWorker2)
            {
                _dbContext.Add(wt);
            }
            _dbContext.SaveChanges();

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
                _repository.GetUserWorkTimes(_worker1, filterForGetEverything).Count,
                _workTimesOfWorker1.Count);

            Assert.AreEqual(
                _repository.GetUserWorkTimes(_worker2, filterForGetEverything).Count,
                _workTimesOfWorker2.Count);

            Assert.AreEqual(_repository.GetUserWorkTimes(_worker1, filterForGetNothing).Count, 0);

            Assert.AreEqual(_repository.GetUserWorkTimes(_worker2, filterForGetNothing).Count, 0);

            Assert.AreEqual(_repository.GetUserWorkTimes(_worker1, filerForGetOnlyWorkTime2).Count, 0);

            Assert.AreEqual(
                _repository.GetUserWorkTimes(_worker2, filerForGetOnlyWorkTime2).Count,
                _workTimesOfWorker2.Count);
        }
        #endregion

        #region EditWorkTimeTests
        [Test]
        public void ShouldChangeWorkTimeWhenDataIsCorrect()
        {
            var newTime = new DbWorkTime
            {
                Id = _id,
                UserId = Guid.NewGuid(),
                StartTime = new DateTime(2020, 1, 1, 1, 1, 1),
                EndTime = new DateTime(2020, 2, 2, 2, 2, 2),
                Title = "ExampleTitle",
                ProjectId = Guid.NewGuid(),
                Description = "Example"
            };

            _dbContext.WorkTimes.Add(_time);
            _dbContext.SaveChanges();

            Assert.True(_repository.EditWorkTime(newTime));

            var result = _dbContext.WorkTimes.Find(_id);

            SerializerAssert.AreEqual(newTime, result);
        }

        [Test]
        public void ShouldThrowExceptionWhenIdIsNotExist()
        {
            Assert.Throws<NotFoundException>(() => _repository.EditWorkTime(_time));
        }
        #endregion
    }
}