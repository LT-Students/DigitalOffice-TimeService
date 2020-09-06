using LT.DigitalOffice.TimeManagementService.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace TimeManagementService.Data.Provider
{
    public interface IDataProvider
    {
        public DbSet<DbLeaveTime> LeaveTimes { get; set; }
        public DbSet<DbWorkTime> WorkTimes { get; set; }
    }
}
