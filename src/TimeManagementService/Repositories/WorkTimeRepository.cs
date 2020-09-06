using LinqKit;
using LT.DigitalOffice.TimeManagementService.Database;
using LT.DigitalOffice.TimeManagementService.Database.Entities;
using LT.DigitalOffice.TimeManagementService.Repositories.Filters;
using LT.DigitalOffice.TimeManagementService.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeManagementService.Repositories
{
    public class WorkTimeRepository : IWorkTimeRepository
    {
        private readonly TimeManagementDbContext dbContext;

        public WorkTimeRepository([FromServices] TimeManagementDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Guid CreateWorkTime(DbWorkTime workTime)
        {
            dbContext.WorkTimes.Add(workTime);
            dbContext.SaveChanges();

            return workTime.Id;
        }

        public ICollection<DbWorkTime> GetUserWorkTimes(Guid userId, WorkTimeFilter filter)
        {
            var predicate = PredicateBuilder.New<DbWorkTime>();

            predicate.Start(wt => wt.WorkerUserId == userId);

            if (filter == null)
            {
                return dbContext.WorkTimes.Where(predicate).ToList();
            }

            if (filter.StartTime != null)
            {
                predicate.And(wt => wt.StartTime >= filter.StartTime);
            }

            if (filter.EndTime != null)
            {
                predicate.And(wt => wt.EndTime <= filter.EndTime);
            }

            return dbContext.WorkTimes.Where(predicate).ToList();
        }

        public bool EditWorkTime(DbWorkTime workTime)
        {
            var time = dbContext.WorkTimes.Find(workTime.Id);

            if (time == null)
            {
                throw new Exception("Work time with this Id is not exist.");
            }

            time.WorkerUserId = workTime.WorkerUserId;
            time.StartTime = workTime.StartTime;
            time.EndTime = workTime.EndTime;
            time.Title = workTime.Title;
            time.ProjectId = workTime.ProjectId;
            time.Description = workTime.Description;

            dbContext.WorkTimes.Update(time);
            dbContext.SaveChanges();

            return true;
        }
    }
}
