using System.Collections.Generic;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
  public record StatInfo
  {
    public DepartmentInfo DepartmentInfo { get; set; }
    public List<UserStatInfo> UsersStats { get; set; }
  }
}
