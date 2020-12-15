using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Interfaces;
using LT.DigitalOffice.TimeManagementService.Mappers.Interfaces;
using LT.DigitalOffice.TimeManagementService.Models.Db;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LT.DigitalOffice.TimeManagementService.Business
{
    public class GetUserLeaveTimesCommand : IGetUserLeaveTimesCommand
    {
        private readonly ILeaveTimeRepository repository;
        private readonly IMapper<DbLeaveTime, LeaveTime> mapper;

        public GetUserLeaveTimesCommand(
            [FromServices] ILeaveTimeRepository repository,
            [FromServices] IMapper<DbLeaveTime, LeaveTime> mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public IEnumerable<LeaveTime> Execute(Guid userId)
        {
            return repository.GetUserLeaveTimes(userId).Select(dblt => mapper.Map(dblt));
        }
    }
}
