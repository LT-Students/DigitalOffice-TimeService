using FluentValidation;
using LT.DigitalOffice.TimeService.Business.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using System;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Business
{
    public class CreateLeaveTimeCommand : ICreateLeaveTimeCommand
    {
        private readonly ICreateLeaveTimeRequestValidator _validator;
        private readonly IDbLeaveTimeMapper _mapper;
        private readonly ILeaveTimeRepository _repository;

        public CreateLeaveTimeCommand(
            ICreateLeaveTimeRequestValidator validator,
            IDbLeaveTimeMapper mapper,
            ILeaveTimeRepository repository)
        {
            _validator = validator;
            _mapper = mapper;
            _repository = repository;
        }

        public Guid Execute(CreateLeaveTimeRequest request)
        {
            var validationResult = _validator.Validate(request);

            if (validationResult != null && !validationResult.IsValid)
            {
                var messages = validationResult.Errors.Select(x => x.ErrorMessage);
                string message = messages.Aggregate((x, y) => x + "\n" + y);

                throw new ValidationException(message);
            }

            _validator.ValidateAndThrow(request);

            var dbLeaveTime = _mapper.Map(request);

            return _repository.CreateLeaveTime(dbLeaveTime);
        }
    }
}
