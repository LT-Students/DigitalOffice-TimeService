using System;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Guid?> CreateAsync(DbWorkTimeDayJob dayJob)
    {
      if (dayJob == null)
      {
        return null;
      }

      _provider.WorkTimeDayJobs.Add(dayJob);
      await _provider.SaveAsync();

      return dayJob.Id;
    }

    public async Task<bool> EditAsync(Guid workTimeDayJobId, JsonPatchDocument<DbWorkTimeDayJob> request)
    {
      if (request == null)
      {
        return false;
      }

      DbWorkTimeDayJob dayJob = await _provider.WorkTimeDayJobs.FirstOrDefaultAsync(dj => dj.Id == workTimeDayJobId);

      if (dayJob == null)
      {
        return false;
      }

      request.ApplyTo(dayJob);
      dayJob.ModifiedAtUtc = DateTime.UtcNow;
      dayJob.ModifiedBy = _httpContextAccessor.HttpContext.GetUserId();
      await _provider.SaveAsync();

      return true;
    }

    public async Task<DbWorkTimeDayJob> GetAsync(Guid workTimeDayJobId, bool includeWorkTime = false)
    {
      var dayJobs = _provider.WorkTimeDayJobs.AsQueryable();

      if (includeWorkTime)
      {
        dayJobs = dayJobs.Include(dj => dj.WorkTime);
      }

      return await dayJobs.FirstOrDefaultAsync(dj => dj.Id == workTimeDayJobId);
    }
  }
}
