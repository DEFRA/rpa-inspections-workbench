using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Web;



namespace RPA.Inspections.Workbench.SL
{

    public class HolidayService : IHolidayService
    {
        public int GetWorkingDays(DateTime start, DateTime end)
        {


            string json;

                IWebProxy proxy = WebRequest.DefaultWebProxy;
                proxy.Credentials = CredentialCache.DefaultCredentials;

                try
                {

                    using (WebClient client = new WebClient())
                    {

                        client.Proxy = proxy;
                        json = client.DownloadString(string.Format("http://d2vmbwa001/defra.bankholidays/api/Values/workingdays?start={0}&end={1}", start.Date.ToString("s"), end.Date.ToString("s")));
                    }

                }
                catch (Exception ex)

                {
                    throw new Exception("Holiday Service is Unavailable", ex);
                }

                int total_days;
                int.TryParse(json, out total_days);

                

                return total_days;
            
        }
    }
}