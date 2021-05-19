using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Models.Dto.Requests.HelpersModels;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Validation.Interfaces
{
    [AutoInject]
    public interface IEditWorkTimeRequestValidator : IValidator<EditWorkTimeModel>
    {
    }
}
