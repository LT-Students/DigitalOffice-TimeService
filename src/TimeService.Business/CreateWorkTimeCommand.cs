using FluentValidation;
using LT.DigitalOffice.TimeService.Business.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Business
{
    public class CreateWorkTimeCommand : ICreateWorkTimeCommand
    {
        private readonly ICreateWorkTimeRequestValidator _validator;
        private readonly ICreateWorkTimeMapper _mapper;
        private readonly IWorkTimeRepository _repository;

        public CreateWorkTimeCommand(
            [FromServices] ICreateWorkTimeRequestValidator validator,
            [FromServices] ICreateWorkTimeMapper mapper,
            [FromServices] IWorkTimeRepository repository)
        {
            _validator = validator;
            _mapper = mapper;
            _repository = repository;
        }

        public Guid Execute(CreateWorkTimeRequest request)
        {
            var validationResult = _validator.Validate(request);

            if (validationResult != null && !validationResult.IsValid)
            {
                var messages = validationResult.Errors.Select(x => x.ErrorMessage);
                string message = messages.Aggregate((x, y) => x + "\n" + y);

                throw new ValidationException(message);
            }

            var dbWorkTime = _mapper.Map(request);

            return _repository.CreateWorkTime(dbWorkTime);
        }
    }
}
