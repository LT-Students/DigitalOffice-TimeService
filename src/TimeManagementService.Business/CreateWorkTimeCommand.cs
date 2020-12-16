using FluentValidation;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Requests;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.TimeManagementService.Business
{
    public class CreateWorkTimeCommand : ICreateWorkTimeCommand
    {
        private readonly IValidator<WorkTimeRequest> validator;
        private readonly IMapper<WorkTimeRequest, DbWorkTime> mapper;
        private readonly IWorkTimeRepository repository;

        public CreateWorkTimeCommand(
            [FromServices] IValidator<WorkTimeRequest> validator,
            [FromServices] IMapper<WorkTimeRequest, DbWorkTime> mapper,
            [FromServices] IWorkTimeRepository repository)
        {
            this.validator = validator;
            this.mapper = mapper;
            this.repository = repository;
        }

        public Guid Execute(WorkTimeRequest request, Guid currentUserId)
        {
            request.CurrentUserId = currentUserId;
            validator.ValidateAndThrowCustom(request);

            var dbWorkTime = mapper.Map(request);

            return repository.CreateWorkTime(dbWorkTime);
        }
    }
}
