using Domain.Entities.Security;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Application.Common.Interface
{
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class RegisterDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

    public class AuthResult
    {
        public bool Succeeded { get; set; }
        public string? ErrorField { get; set; }
        public string? ErrorMessage { get; set; }
        public User? User { get; set; }
        public string? Role { get; set; }

        public static AuthResult Success(User user, string role) =>
            new() { Succeeded = true, User = user, Role = role };
        public static AuthResult Failure(string? field, string message) =>
            new() { Succeeded = false, ErrorField = field, ErrorMessage = message };
    }

    public interface IAccountService
    {
        Task<AuthResult> LoginAsync(LoginDto dto);
        Task<AuthResult> RegisterAsync(RegisterDto dto);
        Task LogoutAsync();
    }
}
