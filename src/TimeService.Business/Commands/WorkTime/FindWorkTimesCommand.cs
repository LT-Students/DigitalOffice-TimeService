using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime
{
    public class FindWorkTimesCommand : IFindWorkTimesCommand
    {
        private readonly IWorkTimeInfoMapper _mapper;
        private readonly IWorkTimeRepository _repository;
        private readonly IAccessValidator _accessValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FindWorkTimesCommand(
            IWorkTimeInfoMapper mapper,
            IWorkTimeRepository repository,
            IAccessValidator accessValidator,
            IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _repository = repository;
            _accessValidator = accessValidator;
            _httpContextAccessor = httpContextAccessor;
        }

        public WorkTimesResponse Execute(FindWorkTimesFilter filter, int skipCount, int takeCount)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var userId = _httpContextAccessor.HttpContext.GetUserId();

            if (filter.UserId.HasValue && filter.UserId != userId && !_accessValidator.IsAdmin())
            {
                throw new ForbiddenException("Not enough rights.");
            }

            int totalCount;

            var dbWorkTimes = _repository.Find(filter, skipCount, takeCount, out totalCount);

            return new WorkTimesResponse
            {
                TotalCount = totalCount,
                Body = dbWorkTimes.Select(_mapper.Map).ToList(),
            };
        }
    }
}
