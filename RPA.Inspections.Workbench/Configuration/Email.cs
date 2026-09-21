using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace RPA.Inspections.Workbench.Configuration
{
    [ExcludeFromCodeCoverage]
    public class Email : ConfigurationElement
    {
        [ConfigurationProperty("address", IsRequired = true)]
        public string Address
        {
            get
            {
                return this["address"] as string;
            }
        }

        [ConfigurationProperty("sender", IsRequired = false)]
        public string Sender
        {
            get
            {
                return this["sender"] as string;
            }
        }
    }


}