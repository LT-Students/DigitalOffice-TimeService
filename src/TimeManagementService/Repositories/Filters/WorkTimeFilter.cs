using System;

namespace LT.DigitalOffice.TimeManagementService.Repositories.Filters
{
    public class WorkTimeFilter
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
