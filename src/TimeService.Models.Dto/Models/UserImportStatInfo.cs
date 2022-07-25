using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
  public class UserImportStatInfo
  {
    public UserData UserData { get; set; }
    public CompanyUserData CompanyUserData { get; set; }
  }
}
