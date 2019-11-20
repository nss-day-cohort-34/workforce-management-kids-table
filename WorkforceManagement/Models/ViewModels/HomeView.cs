using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkforceManagement.Models.ViewModels
{
    public class HomeView
    {
        public int DepartmentId { get; set; }

        public List<Department> Departments { get; set; } = new List<Department>();

        public List<SelectListItem> DepartmentOptions
        {
            get
            {
                if (Departments == null) return null;
                List<SelectListItem> selectItems = Departments.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();
                selectItems.Insert(0, new SelectListItem
                {
                    Text = "Choose Department ...",
                    Value = ""
                });
                return selectItems;
            }
        }

    }
}
