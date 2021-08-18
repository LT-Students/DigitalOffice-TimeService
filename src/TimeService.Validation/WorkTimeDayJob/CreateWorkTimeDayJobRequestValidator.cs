using FluentValidation;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.WorkTimeDayJob.Interfaces;

namespace LT.DigitalOffice.TimeService.Validation.WorkTimeDayJob
{
    public class CreateWorkTimeDayJobRequestValidator : AbstractValidator<CreateWorkTimeDayJobRequest>, ICreateWorkTimeDayJobRequestValidator
    {
        public CreateWorkTimeDayJobRequestValidator()
        {
            RuleFor(wt => wt.Name)
                .Must(n => !string.IsNullOrEmpty(n?.Trim()))
                .WithMessage("Name cannot be empty.");

            RuleFor(wt => wt.Minutes)
                .GreaterThan(0);

            RuleFor(wt => wt.Day)
                .GreaterThan(0)
                .LessThan(32);
        }
    }
}
