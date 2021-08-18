using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Validation.WorkTimeDayJob.Interfaces
{
    [AutoInject]
    public interface IEditWorkTimeDayJobRequestValidator : IValidator<JsonPatchDocument<EditWorkTimeDayJobRequest>>
    {
    }
}
