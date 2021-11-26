using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;

namespace LT.DigitalOffice.TimeService.Validation.Import.Interfaces
{
  [AutoInject]
  public interface IImportStatFilterValidator : IValidator<ImportStatFilter>
  {
  }
}
