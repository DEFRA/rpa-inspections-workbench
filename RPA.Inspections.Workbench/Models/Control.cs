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
    [Table("Control", Schema = "IW")]
    public class Control
    {
        public Guid ControlId { get; set; }


        public string Property { get; set; }

        [Display(Name = "Scheme Year")]
        public string Value { get; set; }

        public bool Active { get; set; }

        public Control()
        {
            ControlId = Guid.NewGuid();
            
        }
    }
}