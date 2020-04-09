using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Models
{
    public class LeaveTypeVM
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Default No of Days")]
        [Range(1, 50, ErrorMessage = "Enter a valid number (1 - 50).")]
        public int DefaultDays { get; set; }

        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }
    }
    
    //public class CreateLeaveTypeVM
    //{
    //    [Required]
    //    public string Name { get; set; }
    //}
}
