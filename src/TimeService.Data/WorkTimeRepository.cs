using LinqKit;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Data
{
    public class WorkTimeRepository : IWorkTimeRepository
    {
        private readonly IDataProvider _provider;

        public WorkTimeRepository(IDataProvider provider)
        {
            _provider = provider;
        }

        public Guid CreateWorkTime(DbWorkTime dbWorkTime)
        {
            _provider.WorkTimes.Add(dbWorkTime);
            _provider.Save();

            return dbWorkTime.Id;
        }

        public ICollection<DbWorkTime> GetUserWorkTimes(Guid userId, WorkTimeFilter filter)
        {
            var predicate = PredicateBuilder.New<DbWorkTime>();

            predicate.Start(wt => wt.UserId == userId);

            if (filter == null)
            {
                return _provider.WorkTimes.Where(predicate).ToList();
            }

            if (filter.StartTime != null)
            {
                predicate.And(wt => wt.StartTime >= filter.StartTime);
            }

            if (filter.EndTime != null)
            {
                predicate.And(wt => wt.EndTime <= filter.EndTime);
            }

            return _provider.WorkTimes.Where(predicate).ToList();
        }

        public bool EditWorkTime(DbWorkTime dbWorkTime)
        {
            var dbWorkTimeToEdit = _provider.WorkTimes.Find(dbWorkTime.Id);

            if (dbWorkTimeToEdit == null)
            {
                throw new NotFoundException($"Work time with Id {dbWorkTime.Id} is not exist.");
            }

            dbWorkTimeToEdit.UserId = dbWorkTime.UserId;
            dbWorkTimeToEdit.StartTime = dbWorkTime.StartTime;
            dbWorkTimeToEdit.EndTime = dbWorkTime.EndTime;
            dbWorkTimeToEdit.Title = dbWorkTime.Title;
            dbWorkTimeToEdit.ProjectId = dbWorkTime.ProjectId;
            dbWorkTimeToEdit.Description = dbWorkTime.Description;

            _provider.WorkTimes.Update(dbWorkTimeToEdit);
            _provider.Save();

            return true;
        }
    }
}
