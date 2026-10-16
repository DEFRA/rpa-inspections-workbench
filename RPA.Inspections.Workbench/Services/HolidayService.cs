using System.Net.Http.Json;

namespace RPA.Inspections.Workbench.SL
{
    public class HolidayService : IHolidayService
    {
        private readonly HttpClient client;
        private readonly string workingDaysUrl;

        public HolidayService(HttpClient client, IConfiguration configuration)
        {
            this.client = client;
            workingDaysUrl = configuration["HolidayService:WorkingDaysUrl"];
        }

        public int GetWorkingDays(DateTime start, DateTime end)
        {
            string json;

            try
            {
                json = client.GetStringAsync(string.Format("{0}?start={1}&end={2}", workingDaysUrl, start.Date.ToString("s"), end.Date.ToString("s"))).GetAwaiter().GetResult();
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

