using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Models
{
    public interface ICattleService
    {
        List<Cattle> GetCattleByHoldingId(Guid? HoldingId);

        List<Cattle> GetLateCattle(Guid? HoldingId);

        List<Cattle> GetCattleByCPH(string CPHNumber);

    }
}
