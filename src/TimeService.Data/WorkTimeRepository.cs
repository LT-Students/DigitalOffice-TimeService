using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Data
{
    public class WorkTimeRepository : IWorkTimeRepository
    {
        private readonly IDataProvider _provider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WorkTimeRepository(
            IDataProvider provider,
            IHttpContextAccessor httpContextAccessor)
        {
            _provider = provider;
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid Create(DbWorkTime dbWorkTime)
        {
            _provider.WorkTimes.Add(dbWorkTime);
            _provider.Save();

            return dbWorkTime.Id;
        }

        public DbWorkTime Get(Guid id)
        {
            var dbWorkTime = _provider.WorkTimes.FirstOrDefault(x => x.Id == id);

            if (dbWorkTime == null)
            {
                throw new NotFoundException($"WorkTime with id {id} was not found.");
            }

            return dbWorkTime;
        }

        public List<DbWorkTime> Find(FindWorkTimesFilter filter, out int totalCount)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (filter.SkipCount < 0)
            {
                throw new BadRequestException("Skip count can't be less than 0.");
            }

            if (filter.TakeCount < 1)
            {
                throw new BadRequestException("Take count can't be less than 1.");
            }

            var dbWorkTimes = _provider.WorkTimes.AsQueryable();

            if (filter.UserId.HasValue)
            {
                dbWorkTimes = dbWorkTimes.Where(x => x.UserId == filter.UserId.Value);
            }

            if (filter.ProjectId.HasValue)
            {
                dbWorkTimes = dbWorkTimes.Where(x => x.ProjectId == filter.ProjectId.Value);
            }

            if (filter.IncludeDayJobs.HasValue && filter.IncludeDayJobs.Value)
            {
                dbWorkTimes = dbWorkTimes.Include(wt => wt.WorkTimeDayJobs.Where(dj => dj.IsActive));
            }

            if (filter.Month.HasValue)
            {
                dbWorkTimes = dbWorkTimes.Where(x => x.Month == filter.Month.Value);
            }

            if (filter.Year.HasValue)
            {
                dbWorkTimes = dbWorkTimes.Where(x => x.Year == filter.Year.Value);
            }

            totalCount = dbWorkTimes.Count();

            return dbWorkTimes.Skip(filter.SkipCount).Take(filter.TakeCount).ToList();
        }

        public bool Edit(DbWorkTime dbWorkTime, JsonPatchDocument<DbWorkTime> jsonPatchDocument)
        {
            jsonPatchDocument.ApplyTo(dbWorkTime);
            dbWorkTime.ModifiedAtUtc = DateTime.UtcNow;
            dbWorkTime.ModifiedBy = _httpContextAccessor.HttpContext.GetUserId();
            _provider.Save();

            return true;
        }

        public DbWorkTime GetLast()
        {
            return _provider.WorkTimes
                .OrderByDescending(wt => wt.Year)
                .ThenByDescending(wt => wt.Month)
                .FirstOrDefault();
        }

        public bool Contains(Guid id)
        {
            return _provider.WorkTimes.Any(wt => wt.Id == id);
        }

    public List<DbWorkTime> Find(List<Guid> usersIds, List<Guid> projectsIds, int year, int month, bool includeJobs = false)
    {
      if (usersIds == null)
      {
        return null;
      }

      IQueryable<DbWorkTime> workTimes = _provider.WorkTimes
        .Where(wt =>
          usersIds.Contains(wt.UserId)
          && projectsIds.Contains(wt.ProjectId)
          && wt.Year == year
          && wt.Month == month);

      if (includeJobs)
      {
        workTimes = workTimes.Include(wt => wt.WorkTimeDayJobs);
      }

      return workTimes.ToList();
    }
  }
}
