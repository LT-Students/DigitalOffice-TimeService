using System;
using System.Collections.Generic;
using System.Text;

namespace LT.DigitalOffice.TimeManagementService.Validation.Interfaces
{
    public interface IAssignValidator
    {
        bool CanAssignUser(Guid currentUserId, Guid assignedUserId);
        bool CanAssignProject(Guid assignedUserId, Guid assignedProjectId);
    }
}
