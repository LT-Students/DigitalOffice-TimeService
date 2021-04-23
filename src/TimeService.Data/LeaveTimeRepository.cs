using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Models.Db;
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

        public Guid CreateLeaveTime(DbLeaveTime dbLeaveTime)
        {
            _provider.LeaveTimes.Add(dbLeaveTime);
            _provider.Save();

            return dbLeaveTime.Id;
        }

        public ICollection<DbLeaveTime> GetUserLeaveTimes(Guid userId)
        {
            return _provider.LeaveTimes
                .Where(lt => lt.UserId == userId)
                .ToList();
        }
    }
}
