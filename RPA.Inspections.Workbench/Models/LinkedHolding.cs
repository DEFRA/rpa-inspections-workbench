using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Models
{
    [ExcludeFromCodeCoverage]
    [Table("LinkedHolding", Schema = "IW")]

    public class LinkedHolding
    {
        public Guid linkedHoldingId { get; set; }
        public Guid holdingId { get; set; }
        public int? locNum { get; set; }
        public string CPHNumberLH { get; set; }
        public string primaryNameLH { get; set; }
        public string secondaryNameLH { get; set; }
        public string addressLine1LH { get; set; }
        public string addressLine2LH { get; set; }
        public string addressLine3LH { get; set; }
        public string regionLH { get; set; }
        public string PostCodeLH { get; set; }
        public bool isLink { get; set; }

        public virtual Holding Holding { get; set; }
    }
}