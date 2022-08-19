using System;
using LT.DigitalOffice.Models.Broker.Models.Company;

namespace LT.DigitalOffice.TimeService.Models.Dto.Models
{
  public record CompanyUserInfo
  {
    public double? Rate { get; set; }
    public ContractSubjectData ContractSubject { get; set; }
    public DateTime StartWorkingAt { get; set; }
  }
}
