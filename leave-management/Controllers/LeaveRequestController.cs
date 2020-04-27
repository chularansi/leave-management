using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace leave_management.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly ILeaveRequestRepository leaveRequestRepo;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUser> userManager;
        private readonly ILeaveTypeRepository leaveTypeRepo;
        private readonly ILeaveAllocationRepository leaveAllocationRepo;

        public LeaveRequestController(ILeaveRequestRepository leaveRequestRepo, IMapper mapper, UserManager<IdentityUser> userManager,
            ILeaveTypeRepository leaveTypeRepo, ILeaveAllocationRepository leaveAllocationRepo)
        {
            this.leaveRequestRepo = leaveRequestRepo;
            this.mapper = mapper;
            this.userManager = userManager;
            this.leaveTypeRepo = leaveTypeRepo;
            this.leaveAllocationRepo = leaveAllocationRepo;
        }

        // GET: LeaveRequest
        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            var leaveRequests = this.leaveRequestRepo.FindAll();
            var leaveRequestModel = this.mapper.Map<List<LeaveRequestVM>>(leaveRequests);

            var model = new AdminLeaveRequestVM
            {
                TotalRequest = leaveRequestModel.Count,
                ApprovedRequest = leaveRequestModel.Count(q => q.Approved == true),
                PendingRequest = leaveRequestModel.Count(q => q.Approved == null),
                RejectedRequest = leaveRequestModel.Count(q => q.Approved == false),
                LeaveRequests = leaveRequestModel
            };
            return View(model);
        }

        public ActionResult MyLeave()
        {
            var empployee = this.userManager.GetUserAsync(User).Result;
            var period = DateTime.Now.Year;
            var employeeAllocations = this.leaveAllocationRepo.FindAll().Where(q => q.EmployeeId == empployee.Id && q.Period == period).ToList();
            var employeeRequests = this.leaveRequestRepo.FindAll().Where(q => q.RequestingEmployeeId == empployee.Id);

            var employeeAllocationModel = this.mapper.Map<List<LeaveAllocationVM>>(employeeAllocations);
            var employeeRequestModel = this.mapper.Map<List<LeaveRequestVM>>(employeeRequests);

            var model = new EmployeeLeaveRequestViewVM
            {
                LeaveAllocations = employeeAllocationModel,
                LeaveRequests = employeeRequestModel
            };

            return View(model);
        }

        // GET: LeaveRequest/Details/5
        public ActionResult Details(int id)
        {
            var leaveRequest = this.leaveRequestRepo.FindById(id);
            var model = this.mapper.Map<LeaveRequestVM>(leaveRequest);
            return View(model);
        }

        public ActionResult ApproveRequest(int id)
        {
            try
            {
                var period = DateTime.Now.Year;
                var user = userManager.GetUserAsync(User).Result;
                var leaveRequest = this.leaveRequestRepo.FindById(id);
                var leaveAllocation = this.leaveAllocationRepo.FindAll().FirstOrDefault(q => q.EmployeeId == leaveRequest.RequestingEmployeeId && q.Period == period && q.LeaveTypeId == leaveRequest.LeaveTypeId);
                int daysRequested = (int)(leaveRequest.EndDate - leaveRequest.StartDate).TotalDays;
                leaveAllocation.NumberOfDays -= daysRequested;
                
                leaveRequest.Approved = true;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

                this.leaveRequestRepo.Update(leaveRequest);
                this.leaveAllocationRepo.Update(leaveAllocation);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index));
            }
        }
        
        public ActionResult RejectRequest(int id)
        {
            try
            {
                var user = userManager.GetUserAsync(User).Result;
                var leaveRequest = this.leaveRequestRepo.FindById(id);
                leaveRequest.Approved = false;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

                var isSuccess = this.leaveRequestRepo.Update(leaveRequest);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: LeaveRequest/Create
        public ActionResult Create()
        {
            var leaveTypes = this.leaveTypeRepo.FindAll();
            var leaveTypeItems = leaveTypes.Select(q => new SelectListItem {
                Text = q.Name,
                Value = q.Id.ToString()
            });

            var model = new CreateLeaveRequestVM
            {
                LeaveTypes = leaveTypeItems
            };
            return View(model);
        }

        // POST: LeaveRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateLeaveRequestVM model)
        {
            try
            {
                var startDate = Convert.ToDateTime(model.StartDate);
                var endDate = Convert.ToDateTime(model.EndDate);

                var leaveTypes = this.leaveTypeRepo.FindAll();
                var leaveTypeItems = leaveTypes.Select(q => new SelectListItem
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                });
                model.LeaveTypes = leaveTypeItems;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if(DateTime.Compare(startDate, endDate) > 1)
                {
                    ModelState.AddModelError("", "Start Date cannot be further in the future than the End Date.");
                    return View(model);
                }

                var employee = this.userManager.GetUserAsync(User).Result;
                var period = DateTime.Now.Year;

                // Get leave allocation by Employee and Leave Type
                var leaveAllocation = this.leaveAllocationRepo.FindAll().FirstOrDefault(q => q.EmployeeId == employee.Id && q.Period == period && q.LeaveTypeId == model.LeaveTypeId);
                int daysRequested = (int)(endDate.Date - endDate.Date).TotalDays;

                if(daysRequested > leaveAllocation.NumberOfDays)
                {
                    ModelState.AddModelError("", "You don't have sufficient days for this request.");
                    return View(model);
                }

                var leaveRequestModel = new LeaveRequestVM
                {
                    RequestingEmployeeId = employee.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    DateActioned = DateTime.Now,
                    LeaveTypeId = model.LeaveTypeId,
                    RequestComments = model.RequestComments
                };

                var leaveRequest = this.mapper.Map<LeaveRequest>(leaveRequestModel);
                var isSuccess = this.leaveRequestRepo.Create(leaveRequest);
                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Something went wrong with submitting your record.");
                    return View(model);
                }
                return RedirectToAction("MyLeave");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong...");
                return View(model);
            }
        }

        public ActionResult CancelRequest(int id)
        {
            var leaveRequest = this.leaveRequestRepo.FindById(id);
            leaveRequest.Cancelled = true;
            this.leaveRequestRepo.Update(leaveRequest);

            return RedirectToAction("MyLeave");
        }

        // GET: LeaveRequest/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveRequest/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequest/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}