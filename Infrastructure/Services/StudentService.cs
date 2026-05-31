using Application.Common.Interface;
using Domain.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class StudentService : IStudentService
    {
        private readonly IApplicationDbContext _context;

        public StudentService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<StudentBasicInfo>> GetPagedStudentsAsync(StudentFilterDto filter)
        {
            var query = _context.StudentBasicInfos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchString))
            {
                query = query.Where(s =>
                    s.FirstName.Contains(filter.SearchString) ||
                    s.LastName.Contains(filter.SearchString));
            }

            if (!string.IsNullOrWhiteSpace(filter.Grade))
            {
                query = query.Where(s => s.Grade == filter.Grade);
            }

            if (!string.IsNullOrWhiteSpace(filter.Gender))
            {
                query = query.Where(s => s.Gender == filter.Gender);
            }

            int totalRecords = await query.CountAsync();
            int pageNumber = Math.Max(1, filter.PageNumber);
            int pageSize = filter.PageSize > 0 ? filter.PageSize : 10;
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalRecords / (double)pageSize));
            pageNumber = Math.Min(pageNumber, totalPages);

            var items = await query
                .OrderBy(s => s.RollNo)
                .ThenBy(s => s.Section)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<StudentBasicInfo>
            {
                Items = items,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<List<string>> GetAllGradesAsync()
        {
            return await _context.StudentBasicInfos
                .Select(s => s.Grade)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();
        }

        public async Task<StudentBasicInfo?> GetByIdAsync(int id)
        {
            return await _context.StudentBasicInfos.FindAsync(id);
        }

        public async Task<ServiceResult> CreateAsync(StudentBasicInfo student)
        {
            var emailExists = await EmailExistsAsync(student.Email);
            if (emailExists)
                return ServiceResult.Failure("Email", "A student with this email address already exists.");

            var rollExists = await RollNoExistsAsync(student.Grade, student.Section, student.RollNo);
            if (rollExists)
                return ServiceResult.Failure("RollNo", "A student with this Roll Number already exists in the same Grade and Section.");

            _context.StudentBasicInfos.Add(student);
            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> UpdateAsync(StudentBasicInfo student)
        {
            var emailExists = await EmailExistsAsync(student.Email, student.Id);
            if (emailExists)
                return ServiceResult.Failure("Email", "Another student with this email address already exists.");

            var rollExists = await RollNoExistsAsync(student.Grade, student.Section, student.RollNo, student.Id);
            if (rollExists)
                return ServiceResult.Failure("RollNo", "Another student with this Roll Number already exists in the same Grade and Section.");

            var existing = await _context.StudentBasicInfos.FindAsync(student.Id);
            if (existing == null)
                return ServiceResult.Failure(string.Empty, "Student not found.");

            existing.FirstName = student.FirstName;
            existing.LastName = student.LastName;
            existing.DateOfBirth = student.DateOfBirth;
            existing.Gender = student.Gender;
            existing.Phone = student.Phone;
            existing.Email = student.Email;
            existing.Address = student.Address;
            existing.Grade = student.Grade;
            existing.Section = student.Section;
            existing.RollNo = student.RollNo;
            existing.ModifiedBy = student.ModifiedBy;
            existing.ModifiedOn = student.ModifiedOn;

            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var student = await _context.StudentBasicInfos.FindAsync(id);
            if (student == null)
                return ServiceResult.Failure(string.Empty, "Student not found.");

            _context.StudentBasicInfos.Remove(student);
            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            return await _context.StudentBasicInfos
                .AnyAsync(s => s.Email == email && (excludeId == null || s.Id != excludeId));
        }

        public async Task<bool> RollNoExistsAsync(string grade, string section, int rollNo, int? excludeId = null)
        {
            return await _context.StudentBasicInfos
                .AnyAsync(s => s.Grade == grade && s.Section == section && s.RollNo == rollNo
                               && (excludeId == null || s.Id != excludeId));
        }
    }
}
