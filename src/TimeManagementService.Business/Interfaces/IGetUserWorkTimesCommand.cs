using LT.DigitalOffice.TimeManagementService.Data.Filters;
using LT.DigitalOffice.TimeManagementService.Models.Dto.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LT.DigitalOffice.TimeManagementService.Business.Interfaces
{
    /// <summary>
    /// Represents interface for a command in command pattern.
    /// Provides method for get user work times.
    /// </summary>
    public interface IGetUserWorkTimesCommand
    {
        /// <summary>
        /// Get leave time of user with id <param name="userId">.
        /// </summary>
        /// <param name="userId">ID of the user who is looking for data.</param>
        /// <param name="filter">Limitations on request.</param>
        /// <returns>Work times with UserId <param name="userId">.</returns>
        public IEnumerable<WorkTime> Execute(Guid userId, WorkTimeFilter filter);
    }
}
