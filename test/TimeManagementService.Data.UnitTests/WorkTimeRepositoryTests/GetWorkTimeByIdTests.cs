using LT.DigitalOffice.Kernel.Exceptions;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Provider;
using LT.DigitalOffice.TimeManagementService.Data.Provider.MsSql.Ef;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.TimeManagementService.Data.UnitTests.WorkTimeRepositoryTests
{
    public class GetWorkTimeByIdTests
    {
        private IDataProvider provider;
        private IWorkTimeRepository repository;

        private Guid dbWorkTimeId;
        private DbWorkTime dbWorkTime;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var dbOptions = new DbContextOptionsBuilder<TimeManagementDbContext>()
                                    .UseInMemoryDatabase("InMemoryDatabase")
                                    .Options;
            provider = new TimeManagementDbContext(dbOptions);
            repository = new WorkTimeRepository(provider);

            dbWorkTimeId = Guid.NewGuid();

            dbWorkTime = new DbWorkTime
            {
                Id = dbWorkTimeId,
                UserId = Guid.NewGuid()
            };

            provider.WorkTimes.Add(dbWorkTime);
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
        public void ShouldReturnsWorkTimeWhenWorkTimeWithThisIdIsInDb()
        {
            Assert.AreEqual(dbWorkTime, repository.GetWorkTimeById(dbWorkTimeId));
        }

        [Test]
        public void ShouldThrowlExceptionWhenWorkTimeWithThisIdNotExistInDb()
        {
            Assert.Throws<NotFoundException>(() => repository.GetWorkTimeById(Guid.NewGuid()));
        }
    }
}