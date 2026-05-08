using Microsoft.AspNetCore.Identity;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;
using UserService.Domain.Repositories;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Seeding
{
    public class AdminSeeder
    {
        private readonly UserDbContext _db;
        private readonly IIdentityService _identityService;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;
        
        public AdminSeeder(
            UserDbContext db,
            IIdentityService identityService,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IClock clock)
        {
            _db = db;
            _identityService = identityService;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }
        public async Task SeedAdminUserAsync(IServiceProvider services)
        {
            const string roleName = "SuperAdmin";
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
            var configuration = services.GetRequiredService<IConfiguration>();

            var adminEmail = configuration["Admin:Email"] 
                ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            var adminPassword = configuration["Admin:Password"] 
                ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
            var adminUserName = "admin";

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                Log.Warning("Admin seeding skipped: Admin:Email or Admin:Password not configured");
                return;
            }

            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new AppRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                });
            }
            
            var user = await userManager.FindByEmailAsync(adminEmail);

            if (user == null)
            {
                user = new AppUser(adminUserName, adminEmail)
                {
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user, adminPassword);
                Log.Information("Admin user created: {Email}", adminEmail);
            }

            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                await userManager.AddToRoleAsync(user, roleName);
                Log.Information("Admin role assigned to user: {Email}", adminEmail);
            }

            var adminUserId = user.Id;

            var existingDomainUser = await _db.DomainUsers.FirstOrDefaultAsync(u => u.Id == adminUserId);

            if (existingDomainUser is null)
            {
                var email = adminEmail?.Trim();
                var seedUsername = email?.Contains("@") == true
                    ? email.Substring(0, email.IndexOf('@'))
                    : email;
                var usernameVO = Username.Create(seedUsername!);
                var emailVO = Email.Create(user.Email!);

                var domainUser = User.Create(
                    adminUserId,
                    usernameVO,
                    emailVO,
                    PersonName.Create(usernameVO.Value, "Admin"),
                    new DateOnly(2000, 1, 1),
                    _clock.UtcNow);
                await _userRepository.AddAsync(domainUser, default);
                await _identityService.UpdateUserProfileIdAsync(adminUserId, domainUser.Id, default);
                await _unitOfWork.SaveChangesAsync(default);
                Log.Information("Admin domain user profile created");
            }
        }
    }
}