using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace RPA.Inspections.Workbench.Tests.Pages
{
    internal static class PageTestExtensions
    {
        // Gives a PageModel just enough HTTP context to read the query string, use TempData and build redirects.
        public static T WithContext<T>(this T page, string query = "") where T : PageModel
        {
            var http = new DefaultHttpContext();
            http.Request.QueryString = new QueryString(query);
            page.PageContext = new PageContext(new ActionContext(http, new Microsoft.AspNetCore.Routing.RouteData(),
                new Microsoft.AspNetCore.Mvc.RazorPages.CompiledPageActionDescriptor()));
            page.TempData = new TempDataDictionary(http, Mock.Of<ITempDataProvider>());
            return page;
        }
    }
}
