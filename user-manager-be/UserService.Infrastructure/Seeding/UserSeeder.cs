using Microsoft.AspNetCore.Identity;
using Serilog;
using UserService.Application.Interfaces.Services;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Seeding
{
    public class UserSeeder
    {
        private readonly UserDbContext _db;
        private readonly IIdentityService _identityService;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IClock _clock;

        public UserSeeder(
            UserDbContext db,
            IIdentityService identityService,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IClock clock)
        {
            _db = db;
            _identityService = identityService;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _clock = clock;
        }

        public async Task SeedUsersAsync(IServiceProvider services)
        {
            var userManager = (UserManager<AppUser>)services.GetService(typeof(UserManager<AppUser>))!;

            // Demo accounts are SuperAdmins with configured passwords: never create them implicitly
            // outside Development. Opt in explicitly with SEED_DEMO_USERS=true.
            var optIn = _configuration["SEED_DEMO_USERS"];
            var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? _configuration["AppSettings:Environment"];
            var enabled = string.IsNullOrWhiteSpace(optIn)
                ? string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase)
                : bool.TryParse(optIn, out var parsed) && parsed;
            if (!enabled)
            {
                Log.Information("Demo user seeding skipped (set SEED_DEMO_USERS=true to enable).");
                return;
            }
            if (!string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase))
                Log.Warning("SEED_DEMO_USERS is enabled outside Development: demo SuperAdmin accounts are being created.");
            
            // 1. Seed Regular User
            var testUserName = _configuration["SEED_TEST_USER_NAME"] ;
            var testUserEmail = _configuration["SEED_TEST_USER_EMAIL"];
            var testUserPassword = _configuration["SEED_TEST_USER_PASSWORD"];
            await SeedUserAsync(userManager, testUserName!, testUserEmail!, testUserPassword!, RoleNames.SuperAdmin, false);

            // 2. Seed SSO User
            var ssoUserName = _configuration["SEED_SSO_USER_NAME"] ;
            var ssoUserEmail = _configuration["SEED_SSO_USER_EMAIL"] ;
            var ssoUserPassword = _configuration["SEED_SSO_USER_PASSWORD"];
            await SeedUserAsync(userManager, ssoUserName!, ssoUserEmail!, ssoUserPassword!, RoleNames.SuperAdmin, true);
        }

        private async Task SeedUserAsync(
            UserManager<AppUser> userManager, 
            string username, 
            string email, 
            string password, 
            string role,
            bool isSSO)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(username))
            {
                Log.Warning("Demo user seeding skipped for an account with incomplete configuration.");
                return;
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new AppUser(username, email)
                {
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    Log.Information("{Type} user created in Identity: {Email} (ID: {Id})", isSSO ? "SSO" : "Regular", email, user.Id);
                }
                else
                {
                    Log.Error("Failed to create user {Email}: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return;
                }
            }

            var currentUser = await userManager.FindByIdAsync(user.Id.ToString());
            if (currentUser == null)
            {
                Log.Error("User {Email} not found in database after creation attempt.", email);
                return;
            }

            if (!await userManager.IsInRoleAsync(currentUser, role))
            {
                var roleResult = await userManager.AddToRoleAsync(currentUser, role);
                if (roleResult.Succeeded)
                {
                    Log.Information("Role {Role} assigned to user: {Email}", role, email);
                }
                else
                {
                    Log.Error("Failed to assign role {Role} to user {Email}: {Errors}", role, email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }

            var userId = user.Id;
            var existingDomainUser = await _db.DomainUsers.FirstOrDefaultAsync(u => u.Id == userId);

            if (existingDomainUser is null)
            {
                var usernameVO = Username.Create(username);
                var emailVO = Email.Create(email);

                var domainUser = User.Create(
                    userId,
                    Username.Create(user.UserName!),
                    emailVO,
                    PersonName.Create(user.UserName!, ""),
                    DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)),
                    DateTime.UtcNow);
                
                if (isSSO)
                {
                    domainUser.EnableSSO();
                }

                await _userRepository.AddAsync(domainUser, default);
                await _identityService.UpdateUserProfileIdAsync(userId, domainUser.Id, default);
                await _unitOfWork.SaveChangesAsync(default);
                Log.Information("{Type} domain user profile created for {Email}", isSSO ? "SSO" : "Regular", email);
            }
        }
    }
}
