using Application.Common.Interface;
using Domain.Entities.Security;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services 
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AccountService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task<AuthResult> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return AuthResult.Failure(null, "Invalid login attempt. Please check your credentials.");

            if (!user.IsActive)
                return AuthResult.Failure(null, "This user account is inactive.");

            var result = await _signInManager.PasswordSignInAsync(user, dto.Password, dto.RememberMe, lockoutOnFailure: false);
            if (!result.Succeeded)
                return AuthResult.Failure(null, "Invalid login attempt. Please check your credentials.");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.Count > 0 ? roles[0] : "User";

            return AuthResult.Success(user, role);
        }

        public async Task<AuthResult> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return AuthResult.Failure("Email", "Email is already registered.");

            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                PreferredDateFormat = "yyyy-MM-dd",
                EmailConfirmed = true,
                DefaultPassword = dto.Password
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var error = string.Join("; ", System.Linq.Enumerable.Select(result.Errors, e => e.Description));
                return AuthResult.Failure(null, error);
            }

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new IdentityRole<int>(dto.Role));

            await _userManager.AddToRoleAsync(user, dto.Role);
            await _signInManager.SignInAsync(user, isPersistent: false);

            return AuthResult.Success(user, dto.Role);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
