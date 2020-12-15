using LinqKit;
using LT.DigitalOffice.Kernel.Exceptions;
using LT.DigitalOffice.TimeManagementService.Data.Filters;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Provider;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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

            predicate.Start(wt => wt.UserId == userId);

            if (filter == null)
            {
                return provider.WorkTimes.Where(predicate).ToList();
            }

            if (filter.StartTime != null)
            {
                predicate.And(wt => wt.StartDate >= filter.StartTime);
            }

            if (filter.EndTime != null)
            {
                predicate.And(wt => wt.EndDate <= filter.EndTime);
            }

            return provider.WorkTimes.Where(predicate).ToList();
        }

        public bool EditWorkTime(DbWorkTime dbWorkTime)
        {
            var workTimeToEdit = provider.WorkTimes
                .AsNoTracking()
                .FirstOrDefault(p => p.Id == dbWorkTime.Id);

            if (workTimeToEdit == null)
            {
                throw new NotFoundException($"Work time with id {dbWorkTime.Id} is not exist.");
            }

            provider.WorkTimes.Update(dbWorkTime);
            provider.Save();

            return true;
        }

        public DbWorkTime GetWorkTimeById(Guid workTimeId)
        {
            var workTime = provider.WorkTimes.FirstOrDefault(x => x.Id == workTimeId);

            if (workTime == null)
            {
                throw new NotFoundException($"Work time with id {workTimeId} is not exist.");
            }

            return workTime;
        }
    }
}
