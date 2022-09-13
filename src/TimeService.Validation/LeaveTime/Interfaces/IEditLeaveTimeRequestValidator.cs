using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Validation.LeaveTime.Interfaces
{
    [AutoInject]
    public interface IEditLeaveTimeRequestValidator : IValidator<(DbLeaveTime, JsonPatchDocument<EditLeaveTimeRequest>)>
    {
    }
}
