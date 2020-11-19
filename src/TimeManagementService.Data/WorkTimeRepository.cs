using LinqKit;
using LT.DigitalOffice.TimeManagementService.Data.Filters;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Provider;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeManagementService.Data
{
    public class WorkTimeRepository : IWorkTimeRepository
    {
        private readonly IDataProvider provider;

        public WorkTimeRepository(IDataProvider provider)
        {
            this.provider = provider;
        }

        public Guid CreateWorkTime(DbWorkTime workTime)
        {
            provider.WorkTimes.Add(workTime);
            provider.Save();

            return workTime.Id;
        }

        public ICollection<DbWorkTime> GetUserWorkTimes(Guid userId, WorkTimeFilter filter)
        {
            var predicate = PredicateBuilder.New<DbWorkTime>();

            predicate.Start(wt => wt.WorkerUserId == userId);

            if (filter == null)
            {
                return provider.WorkTimes.Where(predicate).ToList();
            }

            if (filter.StartTime != null)
            {
                predicate.And(wt => wt.StartTime >= filter.StartTime);
            }

            if (filter.EndTime != null)
            {
                predicate.And(wt => wt.EndTime <= filter.EndTime);
            }

            return provider.WorkTimes.Where(predicate).ToList();
        }

        public bool EditWorkTime(DbWorkTime workTime)
        {
            var time = provider.WorkTimes.Find(workTime.Id);

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

            provider.WorkTimes.Update(time);
            provider.Save();

            return true;
        }
    }
}
