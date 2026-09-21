using RPA.Inspections.Workbench.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace RPA.Inspections.Workbench.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Holding", Schema = "IW")]

    public class Holding
    {

        public Guid holdingId { get; set; }
        [Display(Name = "CPH Number")]
        [CPHcheck]
        public string cphNumber { get; set; }
        [Display(Name = "SBI")]
        [SBIcheck]
        public int? SBI { get; set; }
        [Display(Name = "Holding Primary Name")]
        public string primaryName { get; set; }
        [Display(Name = "Address")]
        public string addressLine1 { get; set; }
        [Display(Name = "Address Line 2")]
        public string addressLine2 { get; set; }
        [Display(Name = "Address Line 3")]
        public string addressLine3 { get; set; }
        [Display(Name = "Region")]
        public string region { get; set; }
        [Display(Name = "Post Code")]
        public string postCode { get; set; }
        [Display(Name = "Phone Number")]
        public string telephoneNumber { get; set; }
        [Display(Name = "Mobile Phone Number")]
        public string mobileNumber { get; set; }
        [Display(Name = "Fax Number")]
        public string faxNumber { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        [Display(Name = "Inspection Deadline Date")]
        [DataType(DataType.Date)]
        public DateTime? inspectionDeadlineDate { get; set; }

        [Display(Name = "Cattle on Holding")]
        public int? animalsToBeInspected { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        [Display(Name = "Last Data Refresh")]
        public DateTime? lastDataRefresh { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        [Display(Name = "Last Data Request")]
        public DateTime? lastDataRequest { get; set; }
        [Display(Name = "Selection Method")]
        [SelMetCheck]
        public string selectionMethod { get; set; }


        
        [Display(Name = "Scheme Year")]
        [SchemeYrcheck]
        public string schemeYear { get; set; }
        [Display(Name = "Active Inspection?")]

        public bool Active { get; set; }

        private string assigneduser;
                [Display(Name = "Assigned Inspector")]
        
        [RegularExpression(@"^[A-Za-z]{1,2}\d*$", ErrorMessage = "Invalid Staff Number. Please enter one or two letters before the staff number.")]
        public string AssignedUser
        {
            get { return assigneduser; }
            set
            {
                if (AssignedUser != value && value != null)
                {
                    assigneduser = value;
                    AssignedDate = DateTime.Now;
                }
                else if(value == null)
                {
                    assigneduser = null;
                }
                else
                    assigneduser = value;
            }
        }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        [Display(Name = "Assigned Date")]
        public DateTime? AssignedDate { get; set; }

        [Display(Name = "Total Notifications This Year")]
        public int TotalMovesThisYear { get; set; }

        
        [Display(Name = "Scheme Name")]
        public string SchemeName { get; set; }

        //[Required(ErrorMessage = "The Scheme Name field is required")]
        public int? SchemeNameDropId { get; set; }

        public virtual SchemeNameDrop SchemeNameDrop { get; set; }

        [Display(Name = "Scheme Year")]
        public string schemeYearOnly
        {
            get
            {
                return string.Format("20{0}",schemeYear.Substring((schemeYear.Length - 2), 2));
            }
        }

        [Display(Name = "Scheme Name Year Only")]
        public string schemeNameYearOnly
        {
            get
            {
                return string.Format("20{0}", SchemeName.Substring((SchemeName.Length - 2), 2));
            }
        }





        public Holding()
        {
            holdingId = Guid.NewGuid();
            Active = true;
            schemeYear = DateTime.Now.Year.ToString();
        }
    }
}