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
        private readonly IDataProvider provider;

        public LeaveTimeRepository(IDataProvider provider)
        {
            this.provider = provider;
        }

        public Guid CreateLeaveTime(DbLeaveTime leaveTime)
        {
            provider.LeaveTimes.Add(leaveTime);
            provider.Save();

            return leaveTime.Id;
        }

        public ICollection<DbLeaveTime> GetUserLeaveTimes(Guid userId)
        {
            return provider.LeaveTimes
                .Where(lt => lt.WorkerUserId == userId)
                .ToList();
        }
    }
}
