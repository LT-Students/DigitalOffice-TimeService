using System;

namespace LT.DigitalOffice.TimeManagementService.Validation.Interfaces.Helpers
{
    public interface IUserAssignmentValidator
    {
        bool UserCanAssignUser(Guid currentUserId, Guid assignedUserId);
    }
}
