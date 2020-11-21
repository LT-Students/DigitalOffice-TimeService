using FluentValidation;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeManagementService.Business
{
    public class EditWorkTimeCommand: IEditWorkTimeCommand
    {
        private readonly IValidator<(JsonPatchDocument<WorkTime>, Guid)> validator;
        private readonly IWorkTimeRepository repository;
        private readonly IMapper<JsonPatchDocument<WorkTime>, DbWorkTime> mapper;

        public EditWorkTimeCommand(
            [FromServices] IValidator<(JsonPatchDocument<WorkTime>, Guid)> validator,
            [FromServices] IWorkTimeRepository repository)
        {
            this.validator = validator;
            this.repository = repository;
        }

        public bool Execute(Guid workTimeId, JsonPatchDocument<DbWorkTime> patch)
        {
            validator.ValidateAndThrowCustom((patch, workTimeId));

            var dbWorkTime = repository.GetWorkTime(workTimeId);
            patch.ApplyTo(dbWorkTime);

            return repository.EditWorkTime(dbWorkTime);
        }
    }
}
