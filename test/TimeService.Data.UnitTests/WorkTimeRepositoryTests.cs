using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.UnitTestKernel;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
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
        public void SetUp()
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
                    CreatedBy = _worker1,
                    StartTime = DateTime.Now.AddDays(-1),
                    EndTime = DateTime.Now.AddDays(-0.75),
                    CreatedAt = DateTime.Now
                },

                new DbWorkTime
                {
                    Id = Guid.NewGuid(),
                    Title = $"WorkTime",
                    UserId = _worker1,
                    ProjectId = _project2,
                    CreatedBy = _worker1,
                    StartTime = DateTime.Now.AddDays(-0.7),
                    EndTime = DateTime.Now.AddDays(-0.45),
                    CreatedAt = DateTime.Now
                }
            };

            _workTimesOfWorker2 = new List<DbWorkTime>
            {
                new DbWorkTime
                {
                    Id = Guid.NewGuid(),
                    Title = $"WorkTime",
                    UserId = _worker2,
                    CreatedBy = _worker2,
                    ProjectId = _project1,
                    StartTime = DateTime.Now.AddDays(-0.9),
                    EndTime = DateTime.Now.AddDays(-0.65),
                    CreatedAt = DateTime.Now
                }
            };

            _id = Guid.NewGuid();

            _time = new DbWorkTime
            {
                Id = _id,
                UserId = Guid.NewGuid(),
                CreatedBy = Guid.NewGuid(),
                StartTime = new DateTime(2020, 1, 1, 1, 1, 1),
                EndTime = new DateTime(2020, 2, 2, 2, 2, 2),
                CreatedAt = DateTime.Now,
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

        #region Create

        [Test]
        public void SuccessfulAddNewWorkTimeInDb()
        {
            var guidOfNewWorkTime = _repository.Create(_workTimesOfWorker1.First());

            Assert.AreEqual(_workTimesOfWorker1.First().Id, guidOfNewWorkTime);
            Assert.NotNull(_dbContext.WorkTimes.Find(_workTimesOfWorker1.First().Id));
        }

        #endregion

        #region Get

        [Test]
        public void ShouldGetExistingWorkTime()
        {
            foreach (var wt in _workTimesOfWorker1)
            {
                _dbContext.Add(wt);
            }

            _dbContext.SaveChanges();

            var id = _workTimesOfWorker1.First().Id;
            var dbWorkTime = _repository.Get(id);

            Assert.AreEqual(dbWorkTime, _workTimesOfWorker1.First());
        }

        [Test]
        public void ShouldThrowNotFoundExceptionWhenWorkTimeIsNotExist()
        {
            foreach (var wt in _workTimesOfWorker1)
            {
                _dbContext.Add(wt);
            }

            _dbContext.SaveChanges();

            Assert.Throws<NotFoundException>(() => _repository.Get(Guid.NewGuid()));
        }

        #endregion

        #region Find

        [Test]
        public void ShouldSuccessfulFindAll()
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

            int count;

            _repository.Find(
                new FindWorkTimesFilter{},
                0,
                int.MaxValue,
                out count);

            Assert.AreEqual(_workTimesOfWorker1.Count + _workTimesOfWorker2.Count, count);
        }

        [Test]
        public void ShouldSuccessfulFindNothing()
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

            int count;

            _repository.Find(
                new FindWorkTimesFilter {
                    UserId = Guid.NewGuid()
                },
                0,
                int.MaxValue,
                out count);

            Assert.AreEqual(0, count);

            _repository.Find(
                new FindWorkTimesFilter
                {
                    StartTime = DateTime.Now.AddDays(10000),
                    EndTime = DateTime.Now.AddDays(-10000)
                },
                0,
                int.MaxValue,
                out count);

            Assert.AreEqual(0, count);
        }

        [Test]
        public void ShouldReturnCorrectlyCount()
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

            int count;

            _repository.Find(
                new FindWorkTimesFilter
                {
                    UserId = _worker1,
                    StartTime = DateTime.Now.AddDays(-100000),
                    EndTime = DateTime.Now.AddDays(100000)
                },
                0,
                int.MaxValue,
                out count);

            Assert.AreEqual(_workTimesOfWorker1.Count, count);

            _repository.Find(
                new FindWorkTimesFilter
                {
                    UserId = _worker2,
                    StartTime = DateTime.Now.AddDays(-100000),
                    EndTime = DateTime.Now.AddDays(100000)
                },
                0,
                int.MaxValue,
                out count);

            Assert.AreEqual(_workTimesOfWorker2.Count, count);
        }

        #endregion

        #region Edit

        [Test]
        public void ShouldChangeWorkTimeWhenDataIsCorrect()
        {
            _dbContext.WorkTimes.Add(_time);
            _dbContext.SaveChanges();

            var jsonPatchDocument = new JsonPatchDocument<DbWorkTime>(new List<Operation<DbWorkTime>>
                {
                    new Operation<DbWorkTime>(
                        "replace",
                        $"/{nameof(DbWorkTime.Title)}",
                        "",
                        "new title")
                }, new CamelCasePropertyNamesContractResolver());


            Assert.True(_repository.Edit(_time, jsonPatchDocument));

            var result = _dbContext.WorkTimes.Find(_id);

            _time.Title = "new title";

            SerializerAssert.AreEqual(_time, result);
        }

        #endregion
    }
}