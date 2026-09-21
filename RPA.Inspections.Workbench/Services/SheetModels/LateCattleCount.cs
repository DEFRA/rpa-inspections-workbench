using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Models
{
    [ExcludeFromCodeCoverage]
    public class LateCattleCount
    {
        public int FMXC1Count { get; set; }
        public int FMXC2Count { get; set; }
        public int DDXC1Count { get; set; }
        public int DDXC2Count { get; set; }
        public int LRXC1Count { get; set; }
        public int LRXC2Count { get; set; }
        public int IMPXC1Count { get; set; }
        public int IMPXC2Count { get; set; }
        public int TotalMovesThisYear { get; set; }

        public string HEA { get; set; }

        public LateCattleCount()
        {
            FMXC1Count = 0;
            FMXC2Count = 0;
            DDXC1Count = 0;
            DDXC2Count = 0;
            LRXC1Count = 0;
            LRXC2Count = 0;
            IMPXC1Count = 0;
            IMPXC2Count = 0;
        }
    }


}