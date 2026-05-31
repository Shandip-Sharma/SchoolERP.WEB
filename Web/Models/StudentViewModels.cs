using System;
using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class StudentViewModel
    {
        public int Id { get; set; } // Map from BaseEntity's ID or since BaseEntity doesn't have an explicitly named ID, wait! Let's check Domain/Entities/Common/BaseEntity.cs again!
        // Wait, did BaseEntity have an ID field?
        // Let's check BaseEntity.cs:
        // public int CreatedBy { get; set; }
        // public DateTime CreatedOn { get; set; }
        // public int? ModifiedBy { get; set; }
        // public DateTime? ModifiedOn { get; set; }
        // Wait! BaseEntity does NOT contain an ID!
        // Wait, let's look at StudentBasicInfo.cs again to see if it has an ID field.
        // StudentBasicInfo inherits from BaseEntity:
        // public class StudentBasicInfo : BaseEntity
        // {
        //     public string FirstName { get; set; }
        //     ...
        // }
        // Wait! It has NO ID field declared either!
        // Ah! Entity Framework Core requires a primary key. By default, it expects a property named "Id" or "StudentBasicInfoId".
        // Wait, how did they define a primary key?
        // In Domain/Entities/Common/BaseEntity.cs:
        // public abstract class BaseEntity { ... }
        // Wait, there was no ID in BaseEntity!
        // Let's verify if there is an ID field. Let's look at BaseEntity.cs again. Yes, it didn't have one!
        // This is a MAJOR compilation/design bug!
        // If we build, EF Core will fail because StudentBasicInfo has no primary key!
        // We MUST add public int Id { get; set; } to BaseEntity.cs! This is the standard in Clean Architecture!
        // Let's verify this. Yes! An ID is required!
        // Let's check if there is an ID in BaseEntity.cs. No, there wasn't!
        // Let's modify BaseEntity.cs to add public int Id { get; set; } at the top of the properties!
    }

    public class StudentFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-10);

        [Required(ErrorMessage = "Gender selection is required")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [RegularExpression(@"^\+?[0-9]{7,15}$", ErrorMessage = "Phone number must be between 7 and 15 digits")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Grade is required")]
        [StringLength(20, ErrorMessage = "Grade cannot exceed 20 characters")]
        public string Grade { get; set; } = string.Empty;

        [Required(ErrorMessage = "Section is required")]
        [StringLength(10, ErrorMessage = "Section cannot exceed 10 characters")]
        public string Section { get; set; } = string.Empty;

        [Required(ErrorMessage = "Roll Number is required")]
        [Range(1, 1000, ErrorMessage = "Roll number must be between 1 and 1000")]
        [Display(Name = "Roll Number")]
        public int RollNo { get; set; }
    }
}
