using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Data
{
    public class WorkTimeRepository : IWorkTimeRepository
    {
        private readonly IDataProvider _provider;

        public WorkTimeRepository(IDataProvider provider)
        {
            _provider = provider;
        }

        public Guid Create(DbWorkTime dbWorkTime)
        {
            _provider.WorkTimes.Add(dbWorkTime);
            _provider.Save();

            return dbWorkTime.Id;
        }

        public DbWorkTime Get(Guid id)
        {
            var dbWorkTime = _provider.WorkTimes.FirstOrDefault(x => x.Id == id);

            if (dbWorkTime == null)
            {
                throw new NotFoundException($"WorkTime with id {id} was not found.");
            }

            return dbWorkTime;
        }

        public List<DbWorkTime> Find(FindWorkTimesFilter filter, int skipPagesCount, int takeCount, out int totalCount)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var dbWorkTimes = _provider.WorkTimes.AsQueryable();

            if (filter.UserId.HasValue)
            {
                dbWorkTimes = dbWorkTimes.Where(x => x.UserId == filter.UserId);
            }

            if (filter.StartTime.HasValue)
            {
                dbWorkTimes = dbWorkTimes.Where(x => x.StartTime >= filter.StartTime);
            }

            if (filter.EndTime.HasValue)
            {
                dbWorkTimes = dbWorkTimes.Where(x => x.EndTime <= filter.EndTime);
            }

            totalCount = dbWorkTimes.Count();

            return dbWorkTimes.Skip(skipPagesCount * takeCount).Take(takeCount).ToList();
        }

        public bool Edit(DbWorkTime dbWorkTime, JsonPatchDocument<DbWorkTime> jsonPatchDocument)
        {
            jsonPatchDocument.ApplyTo(dbWorkTime);
            _provider.Save();

            return true;
        }
    }
}
