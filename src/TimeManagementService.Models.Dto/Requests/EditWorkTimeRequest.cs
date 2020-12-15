﻿using LT.DigitalOffice.TimeManagementService.Models.Db;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Text.Json.Serialization;

namespace LT.DigitalOffice.TimeManagementService.Models.Dto.Requests
{
    public class EditWorkTimeRequest
    {
        public Guid WorkTimeId { get; set; }
        public JsonPatchDocument<DbWorkTime> Patch { get; set; }
        [JsonIgnore]
        public Guid CurrentUserId { get; set; }
    }
}
