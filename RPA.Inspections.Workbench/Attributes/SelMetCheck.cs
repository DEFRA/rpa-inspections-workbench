using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Attributes
{
    public class SelMetCheck:ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool flag = false;

            if (value == null || value.ToString() == "")
            {
                flag = false;
            }
            else
            {
                flag = true;
            }

            if (!flag)
            {

                ValidationResult result = new ValidationResult("Please Select an Option From Drop Down List");

                return result;
            }
            else
            {
                return null;
            }
        }

    }
}