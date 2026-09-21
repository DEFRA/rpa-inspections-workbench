using RPA.Inspections.Workbench.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace RPA.Inspections.Workbench.Extensions
{
    public static class YearExtensions
    {
        public static Expression<Func<Holding, bool>> MatchesReference(string reference)
        {
            return x => "20" + x.SchemeName.Substring((x.SchemeName.Length - 2), 2) == reference;
        }
    }
}