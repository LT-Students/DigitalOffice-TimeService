using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LT.DigitalOffice.TimeService.Models.Dto.Requests.HelpersModels
{
    public class EditWorkTimeModel
    {
        public JsonPatchDocument<EditWorkTimeRequest> JsonPatchDocument { get; set; }
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
    }
}
