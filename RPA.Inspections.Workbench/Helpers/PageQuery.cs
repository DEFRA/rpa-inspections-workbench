using Microsoft.AspNetCore.Http;

namespace RPA.Inspections.Workbench.Helpers
{
    // "page" is a reserved Razor Pages route value, so paging values are read from the query string
    // directly to keep the legacy ?page=&pageSize= URLs working.
    public static class PageQuery
    {
        public static int Page(HttpRequest request) =>
            int.TryParse(request.Query["page"], out var page) && page > 0 ? page : 1;

        public static int PageSize(HttpRequest request, int defaultSize) =>
            int.TryParse(request.Query["pageSize"], out var size) && size > 0 ? size : defaultSize;

        public static string PagerUrl(string path, string searchString, int page, int pageSize) =>
            $"{path}?searchstring={System.Uri.EscapeDataString(searchString ?? string.Empty)}&page={page}&pageSize={pageSize}";
    }
}
