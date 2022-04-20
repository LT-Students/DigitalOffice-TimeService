using System.Linq;
using FluentValidation;
using LT.DigitalOffice.Kernel.Validators.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Validation.Stat.Interfaces;

namespace LT.DigitalOffice.TimeService.Validation.Stat
{
  public class FindStatFilterValidator : AbstractValidator<FindStatFilter>, IFindStatFilterValidator
  {
    public FindStatFilterValidator(IBaseFindFilterValidator validator)
    {
      RuleFor(f => f)
        .SetValidator(validator)
        .Must(f => (f.DepartmentsIds is not null && f.DepartmentsIds.Any())
        || f.ProjectId.HasValue)
        .WithMessage("The request must contain either the id of the department or the project.");
    }
  }
}
