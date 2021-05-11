using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Dto.Requests;

namespace LT.DigitalOffice.TimeService.Business.Commands.WorkTime.Interfaces
{
    /// <summary>
    /// Represents interface for a command in command pattern.
    /// Provides method for change work time.
    /// </summary>
    [AutoInject]
    public interface IEditWorkTimeCommand
    {
        /// <summary>
        /// Changes a work time. Returns true if the operation is successful.
        /// </summary>
        /// <param name="request">Work time data.</param>
        /// <returns>True if the operation is successful.</returns>
        bool Execute(EditWorkTimeRequest request);
    }
}
