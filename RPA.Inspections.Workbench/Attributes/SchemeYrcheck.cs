using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Attributes
{
    public class SchemeYrcheck:ValidationAttribute
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
                int year;

                if (int.TryParse(value.ToString(), out year))
                {
                    if (year > 2015 && year <= 2099)
                    {
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                    }
                }
            }

                if (!flag)
                {

                    ValidationResult result = new ValidationResult("Not a Valid Scheme Year. Please Enter Year in Format '2017'");

                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

    }
