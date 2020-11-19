using LT.DigitalOffice.Kernel.Database;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.TimeManagementService.Data.Provider
{
    public interface IDataProvider : IBaseDataProvider
    {
        DbSet<DbLeaveTime> LeaveTimes { get; set; }
        DbSet<DbWorkTime> WorkTimes { get; set; }
    }
}
