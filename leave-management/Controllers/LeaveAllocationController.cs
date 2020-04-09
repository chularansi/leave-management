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

namespace leave_management.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class LeaveAllocationController : Controller
    {
        private readonly ILeaveTypeRepository leaveTypeRepo;
        private readonly ILeaveAllocationRepository leaveAllocationRepo;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUser> userManager;

        public LeaveAllocationController(ILeaveTypeRepository leaveTypeRepo, ILeaveAllocationRepository leaveAllocationRepo, IMapper mapper,
            UserManager<IdentityUser> userManager)
        {
            this.leaveTypeRepo = leaveTypeRepo;
            this.leaveAllocationRepo = leaveAllocationRepo;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        // GET: LeaveAllocation
        public ActionResult Index()
        {
            var leaveTypes = this.leaveTypeRepo.FindAll().ToList();
            var mappedLeaveTypes = this.mapper.Map<List<LeaveType>, List<LeaveTypeVM>>(leaveTypes);
            var model = new CreateLeaveAllocationVM { LeaveTypes = mappedLeaveTypes, NumberUpdated = 0 };
            return View(model);
        }

        public ActionResult SetLeave(int id)
        {
            var leaveType = this.leaveTypeRepo.FindById(id);
            var employees = this.userManager.GetUsersInRoleAsync("Employee").Result;

            foreach (var emp in employees)
            {
                if(!this.leaveAllocationRepo.CheckAllocation(id, emp.Id))
                {
                    var allocation = new LeaveAllocationVM
                    {
                        EmployeeId = emp.Id,
                        LeaveTypeId = id,
                        NumberOfDays = leaveType.DefaultDays,
                        Period = DateTime.Now.Year,
                        DateCreated = DateTime.Now,
                    };

                    var leaveAllocation = this.mapper.Map<LeaveAllocation>(allocation);
                    this.leaveAllocationRepo.Create(leaveAllocation);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public ActionResult ListEmployees()
        {
            var employees = this.userManager.GetUsersInRoleAsync("Employee").Result;
            var model = this.mapper.Map<List<EmployeeVM>>(employees);
            return View(model);
        }

        // GET: LeaveAllocation/Details/5
        public ActionResult Details(string id)
        {
            var employee = this.userManager.FindByIdAsync(id).Result;
            var mappedEmployee = this.mapper.Map<EmployeeVM>(employee);
            var period = DateTime.Now.Year;

            var leaveAllocations = this.leaveAllocationRepo.FindAll().Where(q => q.EmployeeId == id && q.Period == period).ToList();
            var mappedLeaveAllocations = this.mapper.Map<List<LeaveAllocationVM>>(leaveAllocations);

            var model = new ViewAllocationVM
            {
                Employee = mappedEmployee,
                LeaveAllocations = mappedLeaveAllocations
            };

            return View(model);
        }

        // GET: LeaveAllocation/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveAllocation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveAllocation/Edit/5
        public ActionResult Edit(int id)
        {
            var leaveAllocation = this.leaveAllocationRepo.FindById(id);
            var model = this.mapper.Map<EditLeaveAllocationVM>(leaveAllocation);
            return View(model);
        }

        // POST: LeaveAllocation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditLeaveAllocationVM model)
        {
            try
            {
                // TODO: Add update logic here
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                //var allocation = this.mapper.Map<LeaveAllocation>(model);
                var record = this.leaveAllocationRepo.FindById(model.Id);
                record.NumberOfDays = model.NumberOfDays;
                var isSuccess = this.leaveAllocationRepo.Update(record);
                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Error happened when saving...");
                    return View(model);
                }
                return RedirectToAction(nameof(Details), new { id = model.EmployeeId });
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong...");
                return View(model);
            }
        }

        // GET: LeaveAllocation/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveAllocation/Delete/5
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