using FluentValidation;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeManagementService.Business
{
    public class CreateLeaveTimeCommand : ICreateLeaveTimeCommand
    {
        private readonly IValidator<LeaveTime> validator;
        private readonly IMapper<LeaveTime, DbLeaveTime> mapper;
        private readonly ILeaveTimeRepository repository;

        public CreateLeaveTimeCommand(
            [FromServices] IValidator<LeaveTime> validator,
            [FromServices] IMapper<LeaveTime, DbLeaveTime> mapper,
            [FromServices] ILeaveTimeRepository repository)
        {
            this.validator = validator;
            this.mapper = mapper;
            this.repository = repository;
        }

        public Guid Execute(LeaveTime request, Guid currentUserId)
        {
            request.CurrentUserId = currentUserId;
            validator.ValidateAndThrowCustom(request);

            var dbLeaveTime = mapper.Map(request);

            return repository.CreateLeaveTime(dbLeaveTime);
        }
    }
}
