using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace RPA.Inspections.Workbench.Attributes
{
    public class SBIcheck:ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool flag = false;

            if (value == null)
            {
                flag = true;
            }

            else if (value.ToString() == "0")
            {
                flag = true;
            }

            else
            {
                int sbicheck;

                if(int.TryParse(value.ToString(), out sbicheck))
                {
                    if(sbicheck.ToString().Length == 9)
                    {
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                    }
                }
                else
                {
                    flag = false;
                }

            }

            if (!flag)
            {

                ValidationResult result = new ValidationResult("Not a Valid SBI Number. Must be a 9 digit number or left blank");

                return result;
            }
            else
            {
                return null;
            }
        }
    }
}