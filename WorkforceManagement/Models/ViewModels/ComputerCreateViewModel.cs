using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using WorkforceManagement.Models;
using System.Threading.Tasks;

namespace WorkforceManagement.Models.ViewModels
{
    public class ComputerCreateViewModel
    {
        public List<SelectListItem> Employees { get; set; }
        public Computer computer { get; set; }
        public int employeeId { get; set; }

      

        
    }
}
