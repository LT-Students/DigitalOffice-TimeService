using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Data
{
    public class WorkTimeMonthLimitRepository : IWorkTimeMonthLimitRepository
    {
        private readonly IDataProvider _provider;

        public WorkTimeMonthLimitRepository(IDataProvider provider)
        {
            _provider = provider;
        }

        public Guid Add(DbWorkTimeMonthLimit workTimeMonthLimit)
        {
            if (workTimeMonthLimit == null)
            {
                throw new ArgumentNullException(nameof(workTimeMonthLimit));
            }

            _provider.WorkTimeMonthLimits.Add(workTimeMonthLimit);
            _provider.Save();

            return workTimeMonthLimit.Id;
        }

        public bool Edit(Guid workTimeMonthLimitId, JsonPatchDocument<DbWorkTimeMonthLimit> request)
        {
            DbWorkTimeMonthLimit dbWorkTimeMonthLimit = _provider.WorkTimeMonthLimits.FirstOrDefault(l => l.Id == workTimeMonthLimitId)
                ?? throw new NotFoundException($"No worktime month limits with id {workTimeMonthLimitId}");

            request.ApplyTo(dbWorkTimeMonthLimit);
            _provider.Save();

            return true;
        }

        public List<DbWorkTimeMonthLimit> Find(FindWorkTimeMonthLimitsFilter filter, int skipCount, int takeCount, out int totalCount)
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

            var dbWorkTimeMonthLimits = _provider.WorkTimeMonthLimits.AsQueryable();

            if (filter.Month.HasValue)
            {
                dbWorkTimeMonthLimits = dbWorkTimeMonthLimits.Where(x => x.Month == filter.Month.Value);
            }

            if (filter.Year.HasValue)
            {
                dbWorkTimeMonthLimits = dbWorkTimeMonthLimits.Where(x => x.Year == filter.Year.Value);
            }

            totalCount = dbWorkTimeMonthLimits.Count();

            return dbWorkTimeMonthLimits.Skip(skipCount).Take(takeCount).ToList();
        }

        public DbWorkTimeMonthLimit Get(int year, int month)
        {
            return _provider.WorkTimeMonthLimits.FirstOrDefault(l => l.Year == year && l.Month == month);
        }

        public DbWorkTimeMonthLimit GetLast()
        {
            return _provider.WorkTimeMonthLimits
                .OrderByDescending(l => l.Year)
                .ThenByDescending(l => l.Month)
                .FirstOrDefault();
        }
    }
}
