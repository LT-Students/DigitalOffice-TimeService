using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;

namespace LT.DigitalOffice.TimeService.Validation.WorkTimeDayJob.Interfaces
{
    [AutoInject]
    public interface ICreateWorkTimeDayJobRequestValidator : IValidator<CreateWorkTimeDayJobRequest>
    {
    }
}
