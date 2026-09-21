using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.SL
{
    public interface ILinkedHoldingService
    {
        List<LinkedHolding> GetLinkedholdingById(Guid? HoldingId);
        List<LinkedHolding> GetLinkedholdingByCPH(string CPHNumber);
    }
}
