using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Inspectors", Schema = "IW")]
    public class Inspector
    {

        public Guid inspectorId {get;set;}

        [Display(Name = "Staff Number")]
        public string staffNumber { get; set; }

        
        public Inspector()
        {
            inspectorId = Guid.NewGuid();
        }
    }
}