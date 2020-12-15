using System;

namespace LT.DigitalOffice.TimeManagementService.Validation.Interfaces
{
    public interface IAssignUserValidator
    {
        bool CanAssignUser(Guid currentUserId, Guid assignedUserId);
    }
}
