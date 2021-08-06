using LT.DigitalOffice.TimeService.Data.Interfaces;
using LT.DigitalOffice.TimeService.Data.Provider;
using LT.DigitalOffice.TimeService.Models.Db;
using System;
using System.Linq;

namespace LT.DigitalOffice.TimeService.Data
{
    public class WorkTimeMonthLimitRepository : IWorkTimeMonthLimitRepository
    {
        private readonly IDataProvider _provider;

        public WorkTimeMonthLimitRepository(IDataProvider provider)
        {
            _provider = provider;
        }

        public Guid Add(DbWorkTimeMonthLimit workTimeMonthLimit)
        {
            if (workTimeMonthLimit == null)
            {
                throw new ArgumentNullException(nameof(workTimeMonthLimit));
            }

            _provider.WorkTimeMonthLimits.Add(workTimeMonthLimit);
            _provider.Save();

            return workTimeMonthLimit.Id;
        }

        public DbWorkTimeMonthLimit Get(int year, int month)
        {
            return _provider.WorkTimeMonthLimits.FirstOrDefault(l => l.Year == year && l.Month == month);
        }

        public DbWorkTimeMonthLimit GetLast()
        {
            return _provider.WorkTimeMonthLimits
                .OrderByDescending(l => l.Year)
                .ThenByDescending(l => l.Month)
                .FirstOrDefault();
        }
    }
}
