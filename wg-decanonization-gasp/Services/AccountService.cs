using GaspApp.Data;
using GaspApp.Models;
using GaspApp.Models.AccountViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GaspApp.Services
{
    public class AccountService
    {
        private readonly GaspDbContext _dbContext;
        private readonly IPasswordHasher<Account> _passwordHasher;
        public AccountService(GaspDbContext dbContext, IPasswordHasher<Account> passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        public async Task<SignInErrorType?> SignInAsync(HttpContext context, SignInViewModel viewModel)
        {
            var account = _dbContext.Accounts.SingleOrDefault(a => string.Equals(a.Email, viewModel.Email)); // TODO: case-friendly?
            if (account == null)
                return SignInErrorType.NotFound;

            var passwordResult = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, viewModel.Password);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                // TODO: increment failed attempts
                return SignInErrorType.InvalidPassword;
            }

            ClaimsIdentity identity = new ClaimsIdentity(GetClaims(account), CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return null;
        }

        public async Task SignOutAsync(HttpContext context)
        {
            await context.SignOutAsync();
        }

        public void AddDebugSuperUser()
        {
            var root = new Account
            {
                Email = "root@localhost",
                PasswordHash = _passwordHasher.HashPassword(null!, "root"),
                DisplayName = "root"
            };
            _dbContext.Accounts.Add(root);

            _dbContext.SaveChanges();
        }
        public void AddDemoUser()
		{
            var demo = new Account
            {
                Email = "demo@demo",
                PasswordHash = _passwordHasher.HashPassword(null!, "demo"),
                DisplayName = "Demo Account"
            };
            _dbContext.Accounts.Add(demo);

            _dbContext.SaveChanges();
        }

        private IEnumerable<Claim> GetClaims(Account account)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.DisplayName),
                new Claim(ClaimTypes.Email, account.Email)
            };
            // TODO: Role Claims

            return claims;
        }
    }
}
