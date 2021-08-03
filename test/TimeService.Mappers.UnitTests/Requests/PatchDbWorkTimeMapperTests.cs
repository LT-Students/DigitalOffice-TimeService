using LT.DigitalOffice.TimeService.Mappers.Models;
using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;
using LT.DigitalOffice.UnitTestKernel;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.ProjectService.Mappers.UnitTests.RequestsMappers
{
    public class PatchDbWorkTimeMapperTests
    {
        private IPatchDbWorkTimeMapper _mapper;
        private JsonPatchDocument<EditWorkTimeRequest> _request;
        private JsonPatchDocument<DbWorkTime> _dbRequest;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var newTitle = "new title";
            var newDescription = "new description";
            var newProjectId = Guid.NewGuid();
            var newStartTime = DateTime.Now.AddDays(-2);
            var newEndTime = DateTime.Now.AddDays(-1);

            #region requsest data

            _request = new JsonPatchDocument<EditWorkTimeRequest>(new List<Operation<EditWorkTimeRequest>>
            {
                new Operation<EditWorkTimeRequest>(
                    "replace",
                    $"/{nameof(EditWorkTimeRequest.Title)}",
                    "",
                    newTitle),

                new Operation<EditWorkTimeRequest>(
                    "replace",
                    $"/{nameof(EditWorkTimeRequest.Description)}",
                    "",
                    newDescription),

                new Operation<EditWorkTimeRequest>(
                    "replace",
                    $"/{nameof(EditWorkTimeRequest.ProjectId)}",
                    "",
                    newProjectId),

                new Operation<EditWorkTimeRequest>(
                    "replace",
                    $"/{nameof(EditWorkTimeRequest.StartTime)}",
                    "",
                    newStartTime),

                new Operation<EditWorkTimeRequest>(
                    "replace",
                    $"/{nameof(EditWorkTimeRequest.EndTime)}",
                    "",
                    newEndTime)
            }, new CamelCasePropertyNamesContractResolver());

            #endregion

            #region response data

            _dbRequest = new JsonPatchDocument<DbWorkTime>(new List<Operation<DbWorkTime>>
            {
                new Operation<DbWorkTime>(
                    "replace",
                    $"/{nameof(DbWorkTime.Title)}",
                    "",
                    newTitle),

                new Operation<DbWorkTime>(
                    "replace",
                    $"/{nameof(DbWorkTime.Description)}",
                    "",
                    newDescription),

                new Operation<DbWorkTime>(
                    "replace",
                    $"/{nameof(DbWorkTime.ProjectId)}",
                    "",
                    newProjectId),

                new Operation<DbWorkTime>(
                    "replace",
                    $"/{nameof(DbWorkTime.StartTime)}",
                    "",
                    newStartTime),

                new Operation<DbWorkTime>(
                    "replace",
                    $"/{nameof(DbWorkTime.EndTime)}",
                    "",
                    newEndTime)
            }, new CamelCasePropertyNamesContractResolver());

            #endregion

            _mapper = new PatchDbWorkTimeMapper();
        }

        [Test]
        public void SuccessMap()
        {
            SerializerAssert.AreEqual(_dbRequest, _mapper.Map(_request));
        }

        [Test]
        public void ThrowNullArqumentException()
        {
            JsonPatchDocument<EditWorkTimeRequest> nullRequest = null;
            Assert.Throws<ArgumentNullException>(() => _mapper.Map(nullRequest));
        }
    }
}
