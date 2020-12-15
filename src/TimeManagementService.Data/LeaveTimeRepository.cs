using LT.DigitalOffice.Kernel.Exceptions;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Provider;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

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
            provider.Save();

            return leaveTime.Id;
        }

        public ICollection<DbLeaveTime> GetUserLeaveTimes(Guid userId)
        {
            return provider.LeaveTimes
                .Where(lt => lt.UserId == userId)
                .ToList();
        }

        public bool EditLeaveTime(DbLeaveTime dbLeaveTime)
        {
            var leaveTimeToEdit = provider.LeaveTimes
                .AsNoTracking()
                .FirstOrDefault(p => p.Id == dbLeaveTime.Id);

            if (leaveTimeToEdit == null)
            {
                throw new NotFoundException($"Leave time with id {dbLeaveTime.Id} is not exist.");
            }

            provider.LeaveTimes.Update(dbLeaveTime);
            provider.Save();

            return true;
        }

        public DbLeaveTime GetLeaveTimeById(Guid leaveTimeId)
        {
            var leaveTime = provider.LeaveTimes.FirstOrDefault(x => x.Id == leaveTimeId);

            if (leaveTime == null)
            {
                throw new NotFoundException($"Leave time with id {leaveTimeId} is not exist.");
            }

            return leaveTime;
        }
    }
}
