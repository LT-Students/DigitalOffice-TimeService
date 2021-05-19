using LT.DigitalOffice.TimeService.Models.Dto.Models;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Models.Dto.Responses
{
    public class WorkTimesResponse
    {
        public int TotalCount { get; set; }
        public IEnumerable<WorkTimeInfo> Body { get; set; } = new List<WorkTimeInfo>();
    }
}
