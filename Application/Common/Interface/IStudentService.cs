using Domain.Entities.Student;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Common.Interface
{
    public class StudentFilterDto
    {
        public string? SearchString { get; set; }
        public string? Grade { get; set; }
        public string? Gender { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class ServiceResult
    {
        public bool Succeeded { get; set; }
        public string? ErrorField { get; set; }
        public string? ErrorMessage { get; set; }

        public static ServiceResult Success() => new() { Succeeded = true };
        public static ServiceResult Failure(string field, string message) =>
            new() { Succeeded = false, ErrorField = field, ErrorMessage = message };
    }

    public interface IStudentService
    {
        Task<PagedResult<StudentBasicInfo>> GetPagedStudentsAsync(StudentFilterDto filter);
        Task<List<string>> GetAllGradesAsync();
        Task<StudentBasicInfo?> GetByIdAsync(int id);
        Task<ServiceResult> CreateAsync(StudentBasicInfo student);
        Task<ServiceResult> UpdateAsync(StudentBasicInfo student);
        Task<ServiceResult> DeleteAsync(int id);
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
        Task<bool> RollNoExistsAsync(string grade, string section, int rollNo, int? excludeId = null);
    }
}
