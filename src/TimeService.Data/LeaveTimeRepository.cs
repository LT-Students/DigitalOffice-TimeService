using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Data
{
    public class LeaveTimeRepository : ILeaveTimeRepository
    {
        private readonly IDataProvider _provider;

        public LeaveTimeRepository(IDataProvider provider)
        {
            _provider = provider;
        }

        public Guid Add(DbLeaveTime dbLeaveTime)
        {
            _provider.LeaveTimes.Add(dbLeaveTime);
            _provider.Save();

            return dbLeaveTime.Id;
        }

        public List<DbLeaveTime> Find(FindLeaveTimesFilter filter, int skipCount, int takeCount, out int totalCount)
        {
            if (skipCount < 0)
            {
                throw new BadRequestException("Skip count can't be less than 0.");
            }

            if (takeCount <= 0)
            {
                throw new BadRequestException("Take count can't be equal or less than 0.");
            }

            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var dbLeaveTimes = _provider.LeaveTimes.AsQueryable();

            if (filter.UserId.HasValue)
            {
                dbLeaveTimes = dbLeaveTimes.Where(x => x.UserId == filter.UserId);
            }

            if (filter.StartTime.HasValue)
            {
                dbLeaveTimes = dbLeaveTimes.Where(x => x.StartTime >= filter.StartTime);
            }

            if (filter.EndTime.HasValue)
            {
                dbLeaveTimes = dbLeaveTimes.Where(x => x.EndTime <= filter.EndTime);
            }

            totalCount = dbLeaveTimes.Count();

            return dbLeaveTimes.Skip(skipCount).Take(takeCount).ToList();
        }
    }
}
