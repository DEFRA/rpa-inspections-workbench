using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RPA.Inspections.Workbench.SL
{
    public interface IFileService
    {

        void RunAddList(HttpPostedFileBase bulkSource, bool deleteAll);

        void AddToHolding(DataRow row);

        DataTable BulkData(HttpPostedFileBase bulkSource);

        void ClearTables();

        byte[] StreamFile(string savepath);

        void RunAllBelow(Guid HoldingId);

        void UpdatePackReq(Holding model, PackRequested prmodel, string savepath);

        string InspectionPackCreate(Holding model, PackRequested prmodel);
        string PathBuilder(string file);

        string SafeCPHBuilder(string cph);

    }
}
