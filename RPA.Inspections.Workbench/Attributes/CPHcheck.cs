using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace RPA.Inspections.Workbench.Attributes
{
    public class CPHcheck:ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool flag = false;

            
            if (value == null)
            {
                flag = false;
            }
            else
            {
                if(Regex.IsMatch(value.ToString(), "^[0-9][0-9]/[0-9][0-9][0-9]/[0-9][0-9][0-9][0-9]") && value.ToString().Count() == 11)
                {
                        flag = true;
                }
                else
                {
                    flag = false;
                }

            }

            if (!flag)
            {

                ValidationResult result = new ValidationResult("Not a Valid CPH Number. Must be in Format 00/000/0000");

                return result;
            }
            else
            {
                return null;
            }
        }
    }
}