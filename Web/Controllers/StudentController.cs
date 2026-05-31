using Application.Common.Interface;
using Domain.Entities.Security;
using Domain.Entities.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly UserManager<User> _userManager;

        public StudentController(IStudentService studentService, UserManager<User> userManager)
        {
            _studentService = studentService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(
            string? searchString,
            string? grade,
            string? gender,
            int pageNumber = 1,
            int pageSize = 10)
        {
            ViewBag.SearchString = searchString;
            ViewBag.Grade = grade;
            ViewBag.Gender = gender;
            ViewBag.PageSize = pageSize;

            var filter = new StudentFilterDto
            {
                SearchString = searchString,
                Grade = grade,
                Gender = gender,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _studentService.GetPagedStudentsAsync(filter);

            ViewBag.PageNumber = result.PageNumber;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalRecords = result.TotalRecords;
            ViewBag.Grades = await _studentService.GetAllGradesAsync();

            return View(result.Items);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return PartialView(new StudentFormViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            int currentUserId = currentUser?.Id ?? 0;

            var student = new StudentBasicInfo
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                Phone = model.Phone,
                Email = model.Email,
                Address = model.Address,
                Grade = model.Grade,
                Section = model.Section,
                RollNo = model.RollNo,
                CreatedBy = currentUserId,
                CreatedOn = DateTime.UtcNow
            };

            var result = await _studentService.CreateAsync(student);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(result.ErrorField ?? string.Empty, result.ErrorMessage ?? "An error occurred.");
                return PartialView(model);
            }

            TempData["SuccessMessage"] = "Student registered successfully!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student == null)
                return NotFound();

            var model = new StudentFormViewModel
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender,
                Phone = student.Phone,
                Email = student.Email,
                Address = student.Address,
                Grade = student.Grade,
                Section = student.Section,
                RollNo = student.RollNo
            };

            return PartialView(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentFormViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return PartialView(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            int currentUserId = currentUser?.Id ?? 0;

            var student = new StudentBasicInfo
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                Phone = model.Phone,
                Email = model.Email,
                Address = model.Address,
                Grade = model.Grade,
                Section = model.Section,
                RollNo = model.RollNo,
                ModifiedBy = currentUserId,
                ModifiedOn = DateTime.UtcNow
            };

            var result = await _studentService.UpdateAsync(student);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(result.ErrorField ?? string.Empty, result.ErrorMessage ?? "An error occurred.");
                return PartialView(model);
            }

            TempData["SuccessMessage"] = "Student details updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student == null)
                return NotFound();

            return PartialView(student);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _studentService.DeleteAsync(id);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Student deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
