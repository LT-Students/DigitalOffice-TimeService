using FluentValidation;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace LT.DigitalOffice.TimeManagementService.Business
{
    public class CreateWorkTimeCommand : ICreateWorkTimeCommand
    {
        private readonly IValidator<WorkTime> validator;
        private readonly IMapper<WorkTime, DbWorkTime> mapper;
        private readonly IWorkTimeRepository repository;

        public CreateWorkTimeCommand(
            [FromServices] IValidator<WorkTime> validator,
            [FromServices] IMapper<WorkTime, DbWorkTime> mapper,
            [FromServices] IWorkTimeRepository repository)
        {
            this.validator = validator;
            this.mapper = mapper;
            this.repository = repository;
        }

        public Guid Execute(WorkTime request)
        {
            var validationResult = validator.Validate(request);

            if (validationResult != null && !validationResult.IsValid)
            {
                var messages = validationResult.Errors.Select(x => x.ErrorMessage);
                string message = messages.Aggregate((x, y) => x + "\n" + y);

                throw new ValidationException(message);
            }

            var dbWorkTime = mapper.Map(request);

            return repository.CreateWorkTime(dbWorkTime);
        }
    }
}
