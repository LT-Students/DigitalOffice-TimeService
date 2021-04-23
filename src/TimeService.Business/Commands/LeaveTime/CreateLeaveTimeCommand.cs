using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.TimeService.Validation.Interfaces;
using System;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime
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
            _validator.ValidateAndThrowCustom(request);

            var dbLeaveTime = _mapper.Map(request);

            return _repository.CreateLeaveTime(dbLeaveTime);
        }
    }
}
