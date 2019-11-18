using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WorkforceManagement.Models.ViewModels
{
    public class TrainingProgramCreateView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        [Display(Name = "Max Attendees")]
        public int MaxAttendees { get; set; }
    }
}
