using FluentValidation;
using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TimeManagementService.Business
{
    public class EditWorkTimeCommand: IEditWorkTimeCommand
    {
        private readonly IValidator<EditWorkTimeRequest> validator;
        private readonly IWorkTimeRepository repository;
        private readonly IMapper<EditWorkTimeRequest, DbWorkTime> mapper;

        public EditWorkTimeCommand(
            [FromServices] IValidator<EditWorkTimeRequest> validator,
            [FromServices] IWorkTimeRepository repository,
            [FromServices] IMapper<EditWorkTimeRequest, DbWorkTime> mapper)
        {
            this.validator = validator;
            this.repository = repository;
            this.mapper = mapper;
        }

        public bool Execute(EditWorkTimeRequest request)
        {
            validator.ValidateAndThrow(request);

            return repository.EditWorkTime(mapper.Map(request));
        }
    }
}
