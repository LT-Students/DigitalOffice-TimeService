using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;

namespace LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces
{
  [AutoInject]
  public interface ICreateWorkTimeRequestValidator : IValidator<CreateWorkTimeRequest>
  {
  }
}
