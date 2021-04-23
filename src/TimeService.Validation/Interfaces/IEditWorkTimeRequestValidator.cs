using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;

namespace LT.DigitalOffice.TimeService.Validation.Interfaces
{
    [AutoInject]
    public interface IEditWorkTimeRequestValidator : IValidator<EditWorkTimeRequest>
    {
    }
}
