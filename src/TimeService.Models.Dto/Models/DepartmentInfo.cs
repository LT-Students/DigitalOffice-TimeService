using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
  public record DepartmentInfo
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
  }
}
