using Microsoft.AspNetCore.JsonPatch;
using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Requests.HelpersModels
{
    public record EditWorkTimeModel
    {
        public JsonPatchDocument<EditWorkTimeRequest> JsonPatchDocument { get; set; }
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
    }
}
