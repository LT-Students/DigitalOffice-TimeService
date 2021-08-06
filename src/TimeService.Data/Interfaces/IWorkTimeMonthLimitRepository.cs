using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TimeService.Models.Db;
using System;

namespace LT.DigitalOffice.TimeService.Data.Interfaces
{
    [AutoInject]
    public interface IWorkTimeMonthLimitRepository
    {
        Guid Add(DbWorkTimeMonthLimit workTimeMonthLimit);

        DbWorkTimeMonthLimit Get(int year, int month);

        DbWorkTimeMonthLimit GetLast();
    }
}
