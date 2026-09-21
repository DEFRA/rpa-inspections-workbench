using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.SL
{
    public interface IPackRequested
    {
        PackRequested GetPackRequestedById(Guid? HoldingId);
        void CreatePackRequested(Guid? HoldingId, string cph);
        void send(string cph, Guid? HoldingId);
        List<PackRequested> GetOutstandingPackRequests();
        List<PackRequested> GetDownloadableRequests();
        void DeletePack(string savePath, Guid packRequested);
        void DeleteExisting(Guid? HoldingId, Guid? PackRequestedId);

    }
}
