using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Validation.WorkTime.Interfaces
{
  [AutoInject]
    public interface IEditWorkTimeRequestValidator : IValidator<(DbWorkTime, JsonPatchDocument<EditWorkTimeRequest>)>
    {
    }
}
