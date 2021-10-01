using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Data
{
    public class WorkTimeDayJobRepository : IWorkTimeDayJobRepository
    {
        private readonly IDataProvider _provider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WorkTimeDayJobRepository(
            IDataProvider provider,
            IHttpContextAccessor httpContextAccessor)
        {
            _provider = provider;
            _httpContextAccessor = httpContextAccessor;
        }

        public bool Contains(Guid id)
        {
            return _provider.WorkTimeDayJobs.Any(dj => dj.Id == id);
        }

        public Guid Create(DbWorkTimeDayJob dayJob)
        {
            if (dayJob == null)
            {
                throw new ArgumentNullException(nameof(dayJob));
            }

            _provider.WorkTimeDayJobs.Add(dayJob);
            _provider.Save();

            return dayJob.Id;
        }

        public bool Edit(Guid workTimeDayJobId, JsonPatchDocument<DbWorkTimeDayJob> request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            DbWorkTimeDayJob dayJob = _provider.WorkTimeDayJobs.FirstOrDefault(dj => dj.Id == workTimeDayJobId)
                ?? throw new NotFoundException($"No WorkTimeDayJob with id {workTimeDayJobId}");

            request.ApplyTo(dayJob);
            dayJob.ModifiedAtUtc = DateTime.UtcNow;
            dayJob.ModifiedBy = _httpContextAccessor.HttpContext.GetUserId();
            _provider.Save();

            return true;
        }

        public DbWorkTimeDayJob Get(Guid workTimeDayJobId, bool includeWorkTime = false)
        {
            var dayJobs = _provider.WorkTimeDayJobs.AsQueryable();

            if (includeWorkTime)
            {
                dayJobs = dayJobs.Include(dj => dj.WorkTime);
            }

            return dayJobs.FirstOrDefault(dj => dj.Id == workTimeDayJobId)
                ?? throw new NotFoundException($"No WorkTimeDayJob with id {workTimeDayJobId}");
        }
    }
}
