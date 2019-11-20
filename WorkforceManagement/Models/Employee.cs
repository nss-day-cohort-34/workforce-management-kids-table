using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WorkforceManagement.Models
{
    public class Employee
    {
        public int Id { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Display(Name = "Supervisor")]
        public bool IsSupervisor { get; set; }
        public string SupervisorStatus
        {
            get
            {
                if (IsSupervisor)
                {
                    return "Yes";
                }
                else
                {
                    return "No";
                }
            }
        }
        [Display(Name = "Department Name")]
        public int DepartmentId { get; set; }
        public Department Department { get; set; }
        public Computer Computer { get; set; }
        public List<TrainingProgram> TrainingPrograms { get; set; } = new List<TrainingProgram>();
        public int TotalTrainingPrograms { get; set; }
    }
}
