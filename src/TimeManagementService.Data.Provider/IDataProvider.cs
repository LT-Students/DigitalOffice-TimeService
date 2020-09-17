using LT.DigitalOffice.TimeManagementService.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.TimeManagementService.Data.Provider
{
    public interface IDataProvider
    {
        DbSet<DbLeaveTime> LeaveTimes { get; set; }
        DbSet<DbWorkTime> WorkTimes { get; set; }

        void SaveChanges();
        object MakeEntityDetached(object obj);
        void EnsureDeleted();
        bool IsInMemory();
    }
}
