using FluentValidation;
using LT.DigitalOffice.TimeService.Business.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Business
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
            var validationResult = validator.Validate(request);

            if (validationResult != null && !validationResult.IsValid)
            {
                var messages = validationResult.Errors.Select(x => x.ErrorMessage);
                string message = messages.Aggregate((x, y) => x + "\n" + y);

                throw new ValidationException(message);
            }

            return repository.EditWorkTime(mapper.Map(request));
        }
    }
}
