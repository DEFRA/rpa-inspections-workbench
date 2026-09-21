using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace RPA.Inspections.Workbench.SL
{
    public class MailService:IMailService
    {
        //Create email to Request Pack

        IWorkbenchContext db;

        private static object lockobject = new object();

        public MailService(IWorkbenchContext db)
        {
            this.db = db;
        }



        //this method is no longer used in 2017 release of workbench as direct CTS access link is used.
        public MailMessage sendPackRequest(string cph, Guid? HoldingId)
        {
            List<MailAddress> emails = new List<MailAddress>();

            PackRequested model = db.PackRequested.Where(x => x.holdingId == HoldingId && x.activeRequest == true).FirstOrDefault();

            //string reqId = PackReqService.createRequestId(HoldingId);
            string emailId = model.Id.ToString();

            MailMessage message = new MailMessage
            {
                From = new MailAddress("rpainternaldevelopmentteamtest@rpa.gsi.gov.uk"),
                //From = new MailAddress("SVC_IDT_SSIS@rpa.gsi.gov.uk"),

                Subject = string.Format("II-" + emailId + "-417"),
                IsBodyHtml = true,
                Body = cph
            };

            //message.To.Add(new MailAddress("ctsdb01@defra.gsi.gov.uk"));

            message.To.Add(new MailAddress("[REDACTED_EMAIL]"));

            return message;
        }


        public MailMessage GenerateInspectorEmail(Holding model, PackRequested prmodel)
        {

            MailMessage message = new MailMessage
            {
                //From = new MailAddress("rpainternaldevelopmentteamtest@rpa.gsi.gov.uk"),
                From = new MailAddress("SVC_IDT_SSIS@rpa.gov.uk"),

                Subject = string.Format("Pack Generated: " + model.cphNumber),
                IsBodyHtml = true,
                Body = string.Format("Animal Inspection Pack for " + model.cphNumber + " requested on: " + prmodel.datePackRequested + " has now been Generated." + "\n \n" + "<a href=\"http://d3vmprwwas001/RPA.Inspections.Workbench/\">Click Here</a> to open workbench.")
            };


            message.To.Add(new MailAddress(prmodel.requestorEmail));

            return message;

        }

    }
}