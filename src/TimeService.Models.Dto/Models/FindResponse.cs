using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
    public class WorkTimesResponse
    {
        public int TotalCount { get; set; }
        public IEnumerable<WorkTimeInfo> Body { get; set; } = new List<WorkTimeInfo>();
    }
}
