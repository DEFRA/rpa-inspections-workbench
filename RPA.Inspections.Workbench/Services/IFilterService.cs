using RPA.Inspections.Workbench.Models;
using System.Collections.Generic;

namespace RPA.Inspections.Workbench.SL
{
    public interface IFilterService
    {
        List<Holding> Filterholdings();
    }
}