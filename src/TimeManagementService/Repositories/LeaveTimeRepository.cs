using LT.DigitalOffice.TimeManagementService.Database;
using LT.DigitalOffice.TimeManagementService.Database.Entities;
using LT.DigitalOffice.TimeManagementService.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeManagementService.Repositories
{
    public class LeaveTimeRepository : ILeaveTimeRepository
    {
        private readonly TimeManagementDbContext dbContext;

        public LeaveTimeRepository([FromServices] TimeManagementDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Guid CreateLeaveTime(DbLeaveTime leaveTime)
        {
            dbContext.LeaveTimes.Add(leaveTime);
            dbContext.SaveChanges();

            return leaveTime.Id;
        }

        public ICollection<DbLeaveTime> GetUserLeaveTimes(Guid userId)
        {
            return dbContext.LeaveTimes
                .Where(lt => lt.WorkerUserId == userId)
                .ToList();
        }
    }
}
