using System;

namespace LT.DigitalOffice.TimeManagementService.Validation.Interfaces.Helpers
{
    public interface IProjectAssignmentValidator
    {
        bool CanAssignUserToProject(Guid assignedUserId, Guid assignedProjectId);
    }
}
