using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;

namespace LT.DigitalOffice.TimeService.Validation.Stat.Interfaces
{
  [AutoInject]
  public interface IFindStatFilterValidator : IValidator<FindStatFilter>
  {
  }
}
