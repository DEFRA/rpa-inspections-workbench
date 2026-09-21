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
    [Table("SchemeNameDrop", Schema = "IW")]
    public class SchemeNameDrop
    {
        public int SchemeNameDropId { get; set; }

        [Display(Name = "Scheme Name")]
        public string Text { get; set; }

        public bool Active { get; set; }

    }
}