using FluentValidation;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace LT.DigitalOffice.TimeManagementService.Business
{
    public class EditLeaveTimeCommand : IEditLeaveTimeCommand
    {
        private readonly IValidator<EditLeaveTimeRequest> validator;
        private readonly ILeaveTimeRepository repository;

        public EditLeaveTimeCommand(
            [FromServices] IValidator<EditLeaveTimeRequest> validator,
            [FromServices] ILeaveTimeRepository repository)
        {
            this.validator = validator;
            this.repository = repository;
        }

        public bool Execute(EditLeaveTimeRequest request, Guid currentUserId)
        {
            request.CurrentUserId = currentUserId;
            validator.ValidateAndThrowCustom(request);

            var dbLeaveTime = repository.GetLeaveTimeById(request.LeaveTimeId);
            request.Patch.ApplyTo(dbLeaveTime);

            return repository.EditLeaveTime(dbLeaveTime);
        }
    }
}
