using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkforceManagement.Models.ViewModels
{
    public class EmployeeAssignTrainingProgramViewModel
    {
        public Employee Employee { get; set; }
        public List<TrainingProgram> AllTrainingPrograms { get; set; } = new List<TrainingProgram>();
        public List<int> SelectedTrainingProgramIds { get; set; } = new List<int>();

        public List<SelectListItem> TrainingProgramOptions
        {
            get
            {
                if (AllTrainingPrograms == null) return null;

                return AllTrainingPrograms
                    .Select(t => new SelectListItem(t.Name, t.Id.ToString()))
                    .ToList();
            }
        }
    }
}
