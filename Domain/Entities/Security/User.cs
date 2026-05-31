using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Security
{ 
    public class User: IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public int? SecurityProfileID { get; set; }
        public DateTime CreatedDate { get; set; }
        public string PreferredDateFormat { get; set; }
        public int? EmployeeID { get; set; }
        public int? StudentID { get; set; }
        public bool HasApprovalPermission { get; set; }
        public string DefaultPassword { get; set; }
    }
}
