using LT.DigitalOffice.TimeService.Models.Dto.Models;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Models.Dto.Responses
{
    public class LeaveTimesResponse
    {
        public int TotalCount { get; set; }
        public IEnumerable<LeaveTimeInfo> Body { get; set; } = new List<LeaveTimeInfo>();
    }
}
