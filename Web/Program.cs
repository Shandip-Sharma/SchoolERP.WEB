using Application.Common.Interface;
using Domain.Entities.Security;
using Domain.Entities.Student;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = services.GetRequiredService<UserManager<User>>();

        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            if (!roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
                roleManager.CreateAsync(new IdentityRole<int>(roleName)).GetAwaiter().GetResult();
        }

        var defaultAdminEmail = "admin@school.com";
        var defaultAdminUser = userManager.FindByEmailAsync(defaultAdminEmail).GetAwaiter().GetResult();
        int adminUserId = 1;

        if (defaultAdminUser == null)
        {
            var admin = new User
            {
                UserName = "admin@school.com",
                Email = defaultAdminEmail,
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                EmailConfirmed = true,
                PreferredDateFormat = "yyyy-MM-dd",
                DefaultPassword = "admin123"
            };
            var result = userManager.CreateAsync(admin, "admin123").GetAwaiter().GetResult();
            if (result.Succeeded)
            {
                userManager.AddToRoleAsync(admin, "Admin").GetAwaiter().GetResult();
                adminUserId = admin.Id;
            }
        }
        else
        {
            adminUserId = defaultAdminUser.Id;
        }

        if (!context.StudentBasicInfos.Any())
        {
            var random = new Random();
            var maleNames = new[] { "Aarav", "Ishaan", "Kabir", "Rohan", "Siddharth", "Rahul", "Aditya", "Dev", "Arjun", "Vijay", "Aayush", "Niranjan", "Samir", "Prabhat", "Sabin", "Manish", "Sunil", "Dipesh", "Sandesh", "Prakash" };
            var femaleNames = new[] { "Aanya", "Diya", "Neha", "Ananya", "Priya", "Riya", "Sneha", "Tanvi", "Kriti", "Shruti", "Sujata", "Pooja", "Alisha", "Kiran", "Bina", "Sita", "Gita", "Maya", "Barsha", "Rupa" };
            var lastNames = new[] { "Sharma", "Verma", "Gupta", "Jha", "Singh", "Joshi", "Adhikari", "Shrestha", "Thapa", "Bhattarai", "Karki", "Basnet", "Maharjan", "Pradhan", "Dahal", "Poudel", "Giri", "Shah", "Yadav", "Acharya" };
            var grades = new[] { "Grade 6", "Grade 7", "Grade 8", "Grade 9", "Grade 10", "Grade 11", "Grade 12" };
            var sections = new[] { "A", "B", "C" };
            var addresses = new[] { "Kathmandu, Nepal", "Lalitpur, Nepal", "Bhaktapur, Nepal", "Pokhara, Nepal", "Biratnagar, Nepal", "Lalitpur, Jhamsikhel", "Kathmandu, Baneshwor", "Bhaktapur, Kausaltar", "Kathmandu, Koteshwor", "Lalitpur, Imadol" };

            var rollCounters = new System.Collections.Generic.Dictionary<string, int>();

            for (int i = 0; i < 100; i++)
            {
                var isMale = random.Next(2) == 0;
                var firstName = isMale
                    ? maleNames[random.Next(maleNames.Length)]
                    : femaleNames[random.Next(femaleNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];
                var gender = isMale ? "Male" : "Female";
                var grade = grades[random.Next(grades.Length)];
                var section = sections[random.Next(sections.Length)];
                var address = addresses[random.Next(addresses.Length)];

                var key = $"{grade}-{section}";
                if (!rollCounters.ContainsKey(key))
                    rollCounters[key] = 1;

                var rollNo = rollCounters[key]++;
                var dob = DateTime.Today.AddYears(-random.Next(12, 19)).AddDays(-random.Next(365));
                var phone = "98" + random.Next(10000000, 99999999).ToString();
                var email = $"{firstName.ToLower()}.{lastName.ToLower()}.{i + 1}@school.com";

                context.StudentBasicInfos.Add(new StudentBasicInfo
                {
                    FirstName = firstName,
                    LastName = lastName,
                    DateOfBirth = dob,
                    Gender = gender,
                    Phone = phone,
                    Email = email,
                    Address = address,
                    Grade = grade,
                    Section = section,
                    RollNo = rollNo,
                    CreatedBy = adminUserId,
                    CreatedOn = DateTime.UtcNow
                });
            }

            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(" An error occurred seeding the DB: " + ex.Message);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
