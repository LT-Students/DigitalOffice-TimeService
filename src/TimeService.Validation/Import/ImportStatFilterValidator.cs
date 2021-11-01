using FluentValidation;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Validation.Import.Interfaces;

namespace LT.DigitalOffice.TimeService.Validation.Stat
{
  public class ImportStatFilterValidator : AbstractValidator<ImportStatFilter>, IImportStatFilterValidator
  {
    public ImportStatFilterValidator()
    {
      RuleFor(f => f)
        .Must(f => !f.DepartmentId.HasValue && f.ProjectId.HasValue
          || f.DepartmentId.HasValue && !f.ProjectId.HasValue)
        .WithMessage("The request must contain either the id of the department or the project.");
    }
  }
}
