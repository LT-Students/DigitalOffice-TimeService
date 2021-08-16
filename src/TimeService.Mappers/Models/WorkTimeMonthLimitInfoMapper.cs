using LT.DigitalOffice.TimeService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TimeService.Models.Db;
using LT.DigitalOffice.TimeService.Models.Dto.Models;
using System;

namespace LT.DigitalOffice.TimeService.Mappers.Models
{
    public class WorkTimeMonthLimitInfoMapper : IWorkTimeMonthLimitInfoMapper
    {
        public WorkTimeMonthLimitInfo Map(DbWorkTimeMonthLimit dbWorkTimeMonthLimit)
        {
            if (dbWorkTimeMonthLimit == null)
            {
                throw new ArgumentNullException(nameof(dbWorkTimeMonthLimit));
            }

            return new WorkTimeMonthLimitInfo
            {
                Id = dbWorkTimeMonthLimit.Id,
                Month = dbWorkTimeMonthLimit.Month,
                Year = dbWorkTimeMonthLimit.Year,
                Holidays = dbWorkTimeMonthLimit.Holidays,
                NormHours = dbWorkTimeMonthLimit.NormHours
            };
        }
    }
}
