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
    [Table("OLA.Managers")]
    public partial class Manager
    {
        [Key]
        [Column(Order = 0)]
        public Guid personId { get; set; }

        [Key]
        [Column(Order = 1)]
        public Guid managerId { get; set; }

        public virtual Person Person { get; set; }
    }
}