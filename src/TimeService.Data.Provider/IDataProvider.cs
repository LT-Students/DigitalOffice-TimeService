using LT.DigitalOffice.Kernel.Database;
using LT.DigitalOffice.TimeService.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.TimeService.Data.Provider
{
    public interface IDataProvider : IBaseDataProvider
    {
        DbSet<DbLeaveTime> LeaveTimes { get; set; }
        DbSet<DbWorkTime> WorkTimes { get; set; }
    }
}
