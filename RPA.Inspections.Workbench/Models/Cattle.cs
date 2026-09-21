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
    [Table("Cattle", Schema = "IW")]

    public class Cattle
    {

        public Guid cattleId { get; set; }
        public Guid holdingId { get; set; }
        [Display(Name = "Ear Tag")]
        public string earTag { get; set; }
        [Display(Name = "List Number")]
        public string listNumber { get; set; }
        [Display(Name = "Breed Code")]
        public Guid breedId { get; set; }
        [Display(Name = "Passport Version")]
        public string passportVersion { get; set; }
        [Display(Name = "Gender")]
        public string gender { get; set; }
        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? birthDate { get; set; }
        [Display(Name = "On Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? onDate { get; set; }
        [Display(Name = "DamID")]
        public string damId { get; set;}
        [Display(Name = "Off Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? offDate { get; set; }
        [Display(Name = "Death Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? deathDate { get; set; }
        [Display(Name = "Late Notification")]
        public bool lateNotification { get; set; }
        [Display(Name = "Late Notification Type")]
        public string lateType { get; set; }
        [Display(Name = "Date of Movement/Death/Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? lateDate { get; set; }
        [Display(Name = "Date Notified of Movement")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? notificationDate { get; set; }

        [Display(Name = "Number of Days Late")]
        public int numberOfDaysLate { get; set; }

        public string lateDisplay
        {
            get
            {
                if (lateNotification == true)
                {
                    return string.Format("LATE");
                }
                else
                    return string.Format("NOT LATE");
            }
        }

        public string birthDateFix
        {
            get
            {
                string birthDateTest = birthDate.ToString();

                if (birthDateTest == "01/01/1950")
                {
                    return string.Format("11 NOV 2011");
                }
                else
                    return birthDateTest;
            }
        }

        public string OnDateFix
        {
            get
            {
                string OnDateTest = onDate.ToString();

                if (OnDateTest == "01/01/1950")
                {
                    return string.Format("11 NOV 2011");
                }
                else
                    return OnDateTest;
            }
        }

        public virtual Breed Breed { get; set; }
        public virtual Holding Holding { get; set; }
    }
}