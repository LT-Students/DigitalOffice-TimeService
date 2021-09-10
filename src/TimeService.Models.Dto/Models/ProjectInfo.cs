using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
  public record ProjectInfo
  {
    public Guid Id { get; set; }
    public Guid? DepartmentId { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string ShortName { get; set; }
    public string ShortDescription { get; set; }
  }
}
