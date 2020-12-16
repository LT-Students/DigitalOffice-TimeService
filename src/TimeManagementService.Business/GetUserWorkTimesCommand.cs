using LT.DigitalOffice.TimeManagementService.Business.Interfaces;
using LT.DigitalOffice.TimeManagementService.Data.Filters;
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
    public class GetUserWorkTimesCommand : IGetUserWorkTimesCommand
    {
        private readonly IWorkTimeRepository repository;
        private readonly IMapper<DbWorkTime, WorkTimeResponse> mapper;

        public GetUserWorkTimesCommand(
            [FromServices] IWorkTimeRepository repository,
            [FromServices] IMapper<DbWorkTime, WorkTimeResponse> mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public IEnumerable<WorkTimeResponse> Execute(Guid userId, WorkTimeFilter filter)
        {
            return repository.GetUserWorkTimes(userId, filter).Select(dbwt => mapper.Map(dbwt));
        }
    }
}
