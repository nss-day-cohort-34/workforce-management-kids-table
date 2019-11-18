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
        public List<Computer> Computers { get; set; } = new List<Computer>();
        public List<SelectListItem> ComputerOptions
        {
            get
            {
                if (Computers == null) return null;

                List<SelectListItem> selectItems = Computers
                    .Select(c => new SelectListItem($"{c.Make} ({c.Manufacturer})", c.Id.ToString()))
                    .ToList();
                selectItems.Insert(0, new SelectListItem
                {
                    Text = "Choose computer...",
                    Value = ""
                });
                return selectItems;
            }
        }
    }
}
