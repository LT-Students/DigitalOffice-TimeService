﻿using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TimeService.Business.Commands.LeaveTime.Interfaces;
using LT.DigitalOffice.TimeService.Data.Filters;
using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Dto.Responses;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Business.Commands.LeaveTime
{
    public class FindLeaveTimesCommand : IFindLeaveTimesCommand
    {
        private readonly ILeaveTimeInfoMapper _mapper;
        private readonly ILeaveTimeRepository _repository;
        private readonly IAccessValidator _accessValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FindLeaveTimesCommand(
            ILeaveTimeInfoMapper mapper,
            ILeaveTimeRepository repository,
            IAccessValidator accessValidator,
            IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _repository = repository;
            _accessValidator = accessValidator;
            _httpContextAccessor = httpContextAccessor;
        }

        public LeaveTimesResponse Execute(FindLeaveTimesFilter filter, int skipPagesCount, int takeCount)
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

            var dbLeaveTimes = _repository.Find(filter, skipPagesCount, takeCount, out int totalCount);

            return new LeaveTimesResponse
            {
                TotalCount = totalCount,
                Body = dbLeaveTimes.Select(_mapper.Map).ToList(),
            };
        }
    }
}
