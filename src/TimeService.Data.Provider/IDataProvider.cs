using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Database;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.TimeService.Data.Provider
{
    [AutoInject(InjectType.Scoped)]
    public interface IDataProvider : IBaseDataProvider
    {
        DbSet<DbLeaveTime> LeaveTimes { get; set; }
        DbSet<DbWorkTime> WorkTimes { get; set; }
        DbSet<DbWorkTimeDayJob> WorkTimeDayJobs { get; set; }
        DbSet<DbWorkTimeMonthLimit> WorkTimeMonthLimits { get; set; }
    }
}
