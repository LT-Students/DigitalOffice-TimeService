using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTimeMonthLimit.Interfaces;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Filters;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTimeMonthLimit
{
    public class FindWorkTimeMonthLimitCommand : IFindWorkTimeMonthLimitCommand
    {
        private readonly IWorkTimeMonthLimitRepository _repository;
        private readonly IWorkTimeMonthLimitInfoMapper _mapper;

        public FindWorkTimeMonthLimitCommand(
            IWorkTimeMonthLimitRepository repository,
            IWorkTimeMonthLimitInfoMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public FindResultResponse<WorkTimeMonthLimitInfo> Execute(FindWorkTimeMonthLimitsFilter filter, int skipCount, int takeCount)
        {
            return new()
            {
                Status = OperationResultStatusType.FullSuccess,
                Body = _repository.Find(filter, skipCount, takeCount, out int total).Select(_mapper.Map).ToList(),
                TotalCount = total,
                Errors = new()
            };
        }
    }
}
