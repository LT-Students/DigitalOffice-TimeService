using FluentValidation;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeManagementService.Business
{
    public class CreateWorkTimeCommand : ICreateWorkTimeCommand
    {
        private readonly IValidator<CreateWorkTimeRequest> validator;
        private readonly IMapper<CreateWorkTimeRequest, DbWorkTime> mapper;
        private readonly IWorkTimeRepository repository;

        public CreateWorkTimeCommand(
            [FromServices] IValidator<CreateWorkTimeRequest> validator,
            [FromServices] IMapper<CreateWorkTimeRequest, DbWorkTime> mapper,
            [FromServices] IWorkTimeRepository repository)
        {
            this.validator = validator;
            this.mapper = mapper;
            this.repository = repository;
        }

        public Guid Execute(CreateWorkTimeRequest request)
        {
            validator.ValidateAndThrow(request);

            var dbWorkTime = mapper.Map(request);

            return repository.CreateWorkTime(dbWorkTime);
        }
    }
}
