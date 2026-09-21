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
    [Table("PackRequested", Schema = "IW")]

    public class PackRequested
    {
        [Key]
        public Guid packRequestedId { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid holdingId { get; set; }
        [Display(Name = "Pack Requested")]
        public bool activeRequest { get; set; }
        [Display(Name = "Date Pack Requested")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, HtmlEncode = false, DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime datePackRequested { get; set; }
        public string requestedBy { get; set; }

        public string requestedByName { get; set; }
        public string requestorEmail { get; set; }
        [Display(Name = "Pack Generated Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, HtmlEncode = false, DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? packGenerated { get; set; }
        public string savePath { get; set; }
        public bool display { get; set; }

        [Display(Name = "Task ID")]
        public string showRef
        {
            get
            {
                return string.Format("CE{0}", Id.ToString());
            }
        }

        public virtual Holding Holding { get; set; }

        //Constructor for Pack Requested

        public PackRequested()
        {
            packRequestedId = Guid.NewGuid();
            activeRequest = true;
            display = true;

        }

        public PackRequested(Guid? HoldingId):this()
        {            
            holdingId = HoldingId.Value;           
            datePackRequested = DateTime.Now;
        }
    }
}