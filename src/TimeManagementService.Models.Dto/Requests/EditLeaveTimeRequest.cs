using LT.DigitalOffice.TimeManagementService.Models.Db;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Text.Json.Serialization;

namespace LT.DigitalOffice.TimeManagementService.Models.Dto.Requests
{
    public class EditLeaveTimeRequest
    {
        public Guid LeaveTimeId { get; set; }
        public JsonPatchDocument<DbLeaveTime> Patch { get; set; }
        [JsonIgnore]
        public Guid CurrentUserId { get; set; }
    }
}
