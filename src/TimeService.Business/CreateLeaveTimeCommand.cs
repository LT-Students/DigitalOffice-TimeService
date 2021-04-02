using FluentValidation;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeManagementService.Business
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
            var validationResult = validator.Validate(request);

            if (validationResult != null && !validationResult.IsValid)
            {
                var messages = validationResult.Errors.Select(x => x.ErrorMessage);
                string message = messages.Aggregate((x, y) => x + "\n" + y);

                throw new ValidationException(message);
            }

            validator.ValidateAndThrow(request);

            var dbLeaveTime = mapper.Map(request);

            return repository.CreateLeaveTime(dbLeaveTime);
        }
    }
}
