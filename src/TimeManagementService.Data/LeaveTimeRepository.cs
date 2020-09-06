using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using TimeManagementService.Data.Provider;

namespace LT.DigitalOffice.TimeManagementService.Data
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
            provider.SaveChanges(); // TODO

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
