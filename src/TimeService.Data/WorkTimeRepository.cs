using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
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

        public List<DbWorkTime> Find(FindWorkTimesFilter filter, int skipCount, int takeCount, out int totalCount)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (skipCount < 0)
            {
                throw new BadRequestException("Skip count can't be less than 0.");
            }

            if (takeCount <= 0)
            {
                throw new BadRequestException("Take count can't be equal or less than 0.");
            }

            var dbWorkTimes = _provider.WorkTimes.AsQueryable();

            if (filter.UserId.HasValue)
            {
                dbWorkTimes = dbWorkTimes.Where(x => x.UserId == filter.UserId.Value);
            }

            if (filter.ProjectId.HasValue)
            {
                dbWorkTimes = dbWorkTimes.Where(x => x.ProjectId == filter.ProjectId.Value);
            }

            if (filter.IncludeDayJobs.HasValue && filter.IncludeDayJobs.Value)
            {
                dbWorkTimes = dbWorkTimes.Include(wt => wt.WorkTimeDayJobs.Where(dj => dj.IsActive));
            }

            totalCount = dbWorkTimes.Count();

            return dbWorkTimes.Skip(skipCount).Take(takeCount).ToList();
        }

        public bool Edit(DbWorkTime dbWorkTime, JsonPatchDocument<DbWorkTime> jsonPatchDocument)
        {
            jsonPatchDocument.ApplyTo(dbWorkTime);
            _provider.Save();

            return true;
        }

        public DbWorkTime GetLast()
        {
            return _provider.WorkTimes
                .OrderByDescending(wt => wt.Year)
                .ThenByDescending(wt => wt.Month)
                .FirstOrDefault();
        }

        public bool Contains(Guid id)
        {
            return _provider.WorkTimes.Any(wt => wt.Id == id);
        }
    }
}
