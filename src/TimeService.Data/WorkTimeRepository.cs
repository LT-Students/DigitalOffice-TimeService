﻿using LinqKit;
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

        public Guid CreateWorkTime(DbWorkTime workTime)
        {
            _provider.WorkTimes.Add(workTime);
            _provider.Save();

            return workTime.Id;
        }

        public ICollection<DbWorkTime> GetUserWorkTimes(Guid userId, WorkTimeFilter filter)
        {
            var predicate = PredicateBuilder.New<DbWorkTime>();

            predicate.Start(wt => wt.WorkerUserId == userId);

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

        public bool EditWorkTime(DbWorkTime workTime)
        {
            var time = _provider.WorkTimes.Find(workTime.Id);

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

            _provider.WorkTimes.Update(time);
            _provider.Save();

            return true;
        }
    }
}