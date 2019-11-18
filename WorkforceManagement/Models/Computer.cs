using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WorkforceManagement.Models
{
    public class Computer
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Manufacturer { get; set; }
        [Display(Name = "Purchase Date")]
        public DateTime PurchaseDate { get; set; }
        [Display(Name = "Decommission Date")]
        public DateTime? DecommissionDate { get; set; }
        public Employee employee { get; set; }

    }
}
