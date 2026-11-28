using X.PagedList;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Helpers;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Services;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace RPA.Inspections.Workbench.SL
{
    public class PackRequestedService : IPackRequested
    {

        IWorkbenchContext db;
        IMailService MailService;
        IUserHelper userHelper;
        ISendService sendService;
        IFileStorage fileStorage;

        private static object lockobject = new object();

        public PackRequestedService(IWorkbenchContext context, IMailService mailService, IUserHelper userHelper, ISendService sendService, IFileStorage fileStorage)
        {
            this.db = context;
            this.MailService = mailService;
            this.userHelper = userHelper;
            this.sendService = sendService;
            this.fileStorage = fileStorage;
        }


        //Get a list of Packs Requested by HoldingID

        public PackRequested GetPackRequestedById(Guid? HoldingId)
        {
            PackRequested model = new PackRequested();

            model = db.PackRequested.Where(x => x.holdingId == HoldingId && x.activeRequest == true).FirstOrDefault();


            return model;
        }

        //Create a new Pack Request when Requested

        public void CreatePackRequested(Guid? HoldingId, string cph)
        {


            lock(lockobject)
            {
                try {

                    List<PackRequested> model = db.PackRequested.Where(x => x.holdingId == HoldingId && x.activeRequest == true).ToList();

                    if (model.Count == 0)
                    {


                        PackRequested pc = new PackRequested(HoldingId);

                        pc.requestedBy = userHelper.CurrentUser().ToUpper();
                        pc.requestedByName = userHelper.CurrentUserName(pc.requestedBy);
                        pc.requestorEmail = userHelper.CurrentUserEmail(pc.requestedBy);


                        db.PackRequested.Add(pc);

                        Holding holding = db.Holding.Where(x => x.holdingId == HoldingId).FirstOrDefault();
                        
                        holding.lastDataRequest = DateTime.Now;

                        db.SetModified(holding);

                        db.SaveChanges();

                        DeleteExisting(HoldingId, pc.packRequestedId);


                        //send(cph, HoldingId);

                    }
                }
                catch(Exception ex)
                {
                    throw new Exception("PR001: Unable to Create Inspection Pack", ex);
                }
            }                  
            
 
        }

        //Create email to Request Pack

        public void send(string cph, Guid? HoldingId)
        {
            sendService.sendEmail(MailService.sendPackRequest(cph, HoldingId));
        }




        //Create List of Outstanding Pack Requests for User

        public List<PackRequested> GetOutstandingPackRequests()
        {

            string UserId = userHelper.CurrentUser();

            List<PackRequested> model = new List<PackRequested>();

            model = db.PackRequested.Include(x => x.Holding).Where(x => x.activeRequest == true && x.requestedBy == UserId).OrderByDescending(x => x.datePackRequested).ToList();

            return model;
        }

        //Create List of Packs Available for Download

       public List<PackRequested> GetDownloadableRequests()
        {

            ThreeDayRule();


            string UserId = userHelper.CurrentUser();

            List<PackRequested> model = new List<PackRequested>();

            model = db.PackRequested.Include(x => x.Holding).Where(x => x.packGenerated != null && x.requestedBy.ToUpper() == UserId.ToUpper() && x.display == true).OrderByDescending(x => x.packGenerated).ToList();


            return model;
        }

        //Delete a Pack

        public void DeletePack(string savePath, Guid packRequested)
        {

            PackRequested model = db.PackRequested.Single(x => x.packRequestedId == packRequested);
            if (!string.Equals(model.savePath, savePath, StringComparison.Ordinal))
                throw new InvalidOperationException("The file reference does not match the recorded pack request.");

            fileStorage.Delete(savePath);

            model.display = false;

            db.SetModified(model);

            db.SaveChanges();

             

        }

        //Stop Displaying Existing Packs with Same holding id

        public void DeleteExisting (Guid? HoldingId, Guid? PackRequestedId)
        {

            string UserId = userHelper.CurrentUser();

            List<PackRequested> model = new List <PackRequested>();

            model = db.PackRequested.Where(x => x.holdingId == HoldingId && x.requestedBy == UserId.ToUpper() && x.packRequestedId != PackRequestedId).ToList();

            if (model.Count != 0)
            {
                foreach (var packreq in model)
                    {
                    packreq.display = false;
                    

                    db.SetModified(packreq);
                    }
            }

            db.SaveChanges();
        }

        //Set packs that are over 3 days old to not display

        public void ThreeDayRule ()
        {

            DateTime ExpireDate = DateTime.Now.AddDays(-3);

            List<PackRequested> model = db.PackRequested.ToList();

            foreach (var packreq in model)
            {
                if (packreq.packGenerated < ExpireDate)
                {
                    packreq.display = false;
                    packreq.activeRequest = false;
                    packreq.packGenerated = DateTime.Now;

                    db.SetModified(packreq);
                }
            }



            db.SaveChanges();
        }
    }
}
