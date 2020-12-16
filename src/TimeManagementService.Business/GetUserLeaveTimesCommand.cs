using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Responses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeManagementService.Business
{
    public class GetUserLeaveTimesCommand : IGetUserLeaveTimesCommand
    {
        private readonly ILeaveTimeRepository repository;
        private readonly IMapper<DbLeaveTime, LeaveTimeResponse> mapper;

        public GetUserLeaveTimesCommand(
            [FromServices] ILeaveTimeRepository repository,
            [FromServices] IMapper<DbLeaveTime, LeaveTimeResponse> mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public IEnumerable<LeaveTimeResponse> Execute(Guid userId)
        {
            return repository.GetUserLeaveTimes(userId).Select(dblt => mapper.Map(dblt));
        }
    }
}
