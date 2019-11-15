using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkforceManagement.Models.ViewModels
{
    public class EmployeeEditViewModel
    {
        public Employee Employee { get; set; }
        public List<Department> Departments { get; set; } = new List<Department>();
        public List<SelectListItem> DepartmentOptions
        {
            get
            {
                if (Departments == null) return null;

                List<SelectListItem> selectItems = Departments
                    .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
                    .ToList();
                return selectItems;
            }
        }
    }
}
