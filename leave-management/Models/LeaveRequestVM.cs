using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Models
{
    public class LeaveRequestVM
    {
        public int Id { get; set; }
        public EmployeeVM RequestingEmployee { get; set; }
        public string RequestingEmployeeId { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public LeaveTypeVM LeaveType { get; set; }
        public int LeaveTypeId { get; set; }
        //public IEnumerable<SelectListItem> LeaveTypes { get; set; }

        [Display(Name = "Date Requested")]
        public DateTime DateRequested { get; set; }
        [Display(Name = "Date Actioned")]
        public DateTime DateActioned { get; set; }
        [Display(Name = "Approval State")]
        public bool? Approved { get; set; }
        public EmployeeVM ApprovedBy { get; set; }
        [Display(Name = "Approver Name")]
        public string ApprovedById { get; set; }
        public bool Cancelled { get; set; }
        [Display(Name = "Employee Comments")]
        [MaxLength(300)]
        public string RequestComments { get; set; }

    }

    public class AdminLeaveRequestVM
    {
        [Display(Name = "Total number of requests")]
        public int TotalRequest { get; set; }
        [Display(Name = "Approved requests")]
        public int ApprovedRequest { get; set; }
        [Display(Name = "Pending requests")]
        public int PendingRequest { get; set; }
        [Display(Name = "Rejected requests")]
        public int RejectedRequest { get; set; }
        public List<LeaveRequestVM> LeaveRequests { get; set; }
    }

    public class CreateLeaveRequestVM
    {
        [Display(Name = "Start Date")]
        [Required]
        public string StartDate { get; set; }
        //public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [Required]
        public string EndDate { get; set; }
        //public DateTime EndDate { get; set; }

        public IEnumerable<SelectListItem> LeaveTypes { get; set; }
        public int LeaveTypeId { get; set; }
        [Display(Name = "Employee Comments")]
        [MaxLength(300)]
        public string RequestComments { get; set; }
    }

    public class EmployeeLeaveRequestViewVM
    {
        public List<LeaveAllocationVM> LeaveAllocations { get; set; }

        public List<LeaveRequestVM> LeaveRequests { get; set; }
    }
}
