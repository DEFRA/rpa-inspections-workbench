using RPA.Inspections.Workbench.Helpers;
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
    [Table("BcmsPackRequested", Schema = "IW")]

    public class BcmsRequested
    {
        [Key]
        public Guid BcmsPackRequestedId { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, HtmlEncode = false, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, HtmlEncode = false, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "CPH Number")]
        public string CphNumber { get; set; }

        [Display(Name = "Scheme Year")]
        public int? SchemeYear { get; set; }

        [Display(Name = "Active Request")]
        public bool ActiveRequest { get; set; }

        [Display(Name = "Date Pack Requested")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, HtmlEncode = false, DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime DatePackRequested { get; set; }

        [Display(Name = "Bulk Request")]
        public bool BulkRequest { get; set; }

        [Display(Name = "Generated Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, HtmlEncode = false, DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? BcmsGenerated { get; set; }

        public string Error { get; set; }

        [Display(Name = "Generation Report")]
        //[Url]
        public string Filepath { get; set; }
    }
}