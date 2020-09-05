using FluentValidation;
using LT.DigitalOffice.TimeManagementService.Commands.Interfaces;
using LT.DigitalOffice.TimeManagementService.Database.Entities;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models;
using LT.DigitalOffice.TimeManagementService.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeManagementService.Commands
{
    public class CreateLeaveTimeCommand : ICreateLeaveTimeCommand
    {
        private readonly IValidator<CreateLeaveTimeRequest> validator;
        private readonly IMapper<CreateLeaveTimeRequest, DbLeaveTime> mapper;
        private readonly ILeaveTimeRepository repository;

        public CreateLeaveTimeCommand(
            [FromServices] IValidator<CreateLeaveTimeRequest> validator,
            [FromServices] IMapper<CreateLeaveTimeRequest, DbLeaveTime> mapper,
            [FromServices] ILeaveTimeRepository repository)
        {
            this.validator = validator;
            this.mapper = mapper;
            this.repository = repository;
        }

        public Guid Execute(CreateLeaveTimeRequest request)
        {
            validator.ValidateAndThrow(request);

            var dbLeaveTime = mapper.Map(request);

            return repository.CreateLeaveTime(dbLeaveTime);
        }
    }
}
