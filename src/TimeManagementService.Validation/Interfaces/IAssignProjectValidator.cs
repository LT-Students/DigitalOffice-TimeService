using System;

namespace LT.DigitalOffice.TimeManagementService.Validation.Interfaces
{
    public interface IAssignProjectValidator
    {
        bool CanAssignProject(Guid assignedUserId, Guid assignedProjectId);
    }
}
