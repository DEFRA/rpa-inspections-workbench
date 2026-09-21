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
    [Table("Breed", Schema = "IW")]

    public class Breed
    {

        public Guid breedId { get; set; }

        private string breedcode;
        [Display(Name = "Breed Code")]
        public string breedCode
        {
            get { return breedcode; }

            set
            {
                if (!string.IsNullOrEmpty(value))
                    breedcode = value.ToUpper();
                else
                    breedcode = value;
            }

        }

        [Display(Name = "Breed Name")]
        public string breedFull { get; set; }
        public bool Active { get; set; }

        public Breed()
        {
            breedId = Guid.NewGuid();
            Active = true;
        }


    }
}