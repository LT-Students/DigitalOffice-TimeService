using LT.DigitalOffice.Models.Broker.Models.Department;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Models;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
  public class DepartmentInfoMapper : IDepartmentInfoMapper
  {
    public DepartmentInfo Map(DepartmentData departmentData)
    {
      return new DepartmentInfo()
      {
        Id = departmentData.Id,
        Name = departmentData.Name
      };
    }
  }
}
