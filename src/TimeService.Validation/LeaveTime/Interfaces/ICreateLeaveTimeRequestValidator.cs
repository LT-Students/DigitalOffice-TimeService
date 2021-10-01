using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;

namespace LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces
{
    [AutoInject]
    public interface ICreateLeaveTimeRequestValidator : IValidator<CreateLeaveTimeRequest>
    {
    }
}
