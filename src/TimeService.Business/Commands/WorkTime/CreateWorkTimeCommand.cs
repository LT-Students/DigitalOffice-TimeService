using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime
{
    public class CreateWorkTimeCommand : ICreateWorkTimeCommand
    {
        private readonly ICreateWorkTimeRequestValidator _validator;
        private readonly IDbWorkTimeMapper _mapper;
        private readonly IWorkTimeRepository _repository;

        public CreateWorkTimeCommand(
            ICreateWorkTimeRequestValidator validator,
            IDbWorkTimeMapper mapper,
            IWorkTimeRepository repository)
        {
            _validator = validator;
            _mapper = mapper;
            _repository = repository;
        }

        public Guid Execute(CreateWorkTimeRequest request)
        {
            _validator.ValidateAndThrowCustom(request);

            var dbWorkTime = _mapper.Map(request);

            return _repository.CreateWorkTime(dbWorkTime);
        }
    }
}
