using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RPA.Inspections.Workbench.Models
{
    [ExcludeFromCodeCoverage]
    [Table("Managers", Schema = "OLA")]
    public partial class Manager
    {
        [Column(Order = 0)]
        public Guid personId { get; set; }

        [Column(Order = 1)]
        public Guid managerId { get; set; }

        public virtual Person Person { get; set; }
    }
}