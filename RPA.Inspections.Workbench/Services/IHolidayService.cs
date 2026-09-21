using System;

namespace RPA.Inspections.Workbench.SL
{
    public interface IHolidayService
    {
        int GetWorkingDays(DateTime start, DateTime end);
    }
}