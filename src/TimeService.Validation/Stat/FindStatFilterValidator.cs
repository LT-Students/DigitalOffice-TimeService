using System.Linq;
using FluentValidation;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Validation.Stat.Interfaces;

namespace LT.DigitalOffice.TimeService.Validation.Stat
{
  public class FindStatFilterValidator : AbstractValidator<FindStatFilter>, IFindStatFilterValidator
  {
    public FindStatFilterValidator()
    {
      RuleFor(f => f)
        .Must(f => (f.DepartmentsIds is not null && f.DepartmentsIds.Any())
        || (f.ProjectsIds is not null && f.ProjectsIds.Any()))
        .WithMessage("The request must contain either the id of the department or the project.");
    }
  }
}
