﻿using LT.DigitalOffice.TimeService.Mappers.Requests.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Requests
{
    public class DbWorkTimeMapper : IDbWorkTimeMapper
    {
        public DbWorkTime Map(EditWorkTimeRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new DbWorkTime()
            {
                Id = request.Id,
                WorkerUserId = request.WorkerUserId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Title = request.Title,
                ProjectId = request.ProjectId,
                Description = request.Description
            };
        }

        public DbWorkTime Map(CreateWorkTimeRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new DbWorkTime
            {
                Id = Guid.NewGuid(),
                WorkerUserId = request.WorkerUserId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Title = request.Title,
                ProjectId = request.ProjectId,
                Description = request.Description
            };
        }
    }
}