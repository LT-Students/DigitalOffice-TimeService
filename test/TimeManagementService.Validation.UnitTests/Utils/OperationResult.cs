using LT.DigitalOffice.Kernel.Broker;
using System.Collections.Generic;

namespace LT.DigitalOffice.TimeManagementService.Validation.UnitTests.Utils
{
    public class OperationResult<T> : IOperationResult<T>
    {
        public bool IsSuccess { get; set; }

        public List<string> Errors { get; set; }

        public T Body { get; set; }
    }
}
