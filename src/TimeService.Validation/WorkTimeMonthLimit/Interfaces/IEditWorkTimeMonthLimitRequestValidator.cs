﻿using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TimeService.Validation.WorkTimeMonthLimit.Interfaces
{
    [AutoInject]
    public interface IEditWorkTimeMonthLimitRequestValidator : IValidator<JsonPatchDocument<EditWorkTimeMonthLimitRequest>>
    {
    }
}
