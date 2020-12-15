using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Filters;
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
    public class GetUserWorkTimesCommand : IGetUserWorkTimesCommand
    {
        private readonly IWorkTimeRepository repository;
        private readonly IMapper<DbWorkTime, WorkTime> mapper;

        public GetUserWorkTimesCommand(
            [FromServices] IWorkTimeRepository repository,
            [FromServices] IMapper<DbWorkTime, WorkTime> mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public IEnumerable<WorkTime> Execute(Guid userId, WorkTimeFilter filter)
        {
            return repository.GetUserWorkTimes(userId, filter).Select(dbwt => mapper.Map(dbwt));
        }
    }
}
