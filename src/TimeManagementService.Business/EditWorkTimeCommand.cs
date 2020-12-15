using FluentValidation;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeManagementService.Business
{
    public class EditWorkTimeCommand: IEditWorkTimeCommand
    {
        private readonly IValidator<EditWorkTimeRequest> validator;
        private readonly IWorkTimeRepository repository;

        public EditWorkTimeCommand(
            [FromServices] IValidator<EditWorkTimeRequest> validator,
            [FromServices] IWorkTimeRepository repository)
        {
            this.validator = validator;
            this.repository = repository;
        }

        public bool Execute(EditWorkTimeRequest request, Guid currentUserId)
        {
            request.CurrentUserId = currentUserId;
            validator.ValidateAndThrowCustom(request);

            var dbWorkTime = repository.GetWorkTimeById(request.WorkTimeId);
            request.Patch.ApplyTo(dbWorkTime);

            return repository.EditWorkTime(dbWorkTime);
        }
    }
}
