using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
    public class WorkTimesResponse
    {
        public int TotalCount { get; set; }
        public IEnumerable<WorkTimeInfo> Body { get; set; } = new List<WorkTimeInfo>();
    }
}
