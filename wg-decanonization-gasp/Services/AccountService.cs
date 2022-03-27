using GaspApp.Data;
using GaspApp.Models;
using GaspApp.Models.AccountViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace GaspApp.Services
{
    public class AccountService
    {
        private readonly GaspDbContext _dbContext;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly SendGridClient _sendGridClient;
		public AccountService(GaspDbContext dbContext, IPasswordHasher<Account> passwordHasher, SendGridClient sendGridClient)
		{
			_dbContext = dbContext;
			_passwordHasher = passwordHasher;
			_sendGridClient = sendGridClient;
		}
		public async Task<SignInErrorType?> CreateSignInToken(string email)
		{
            var account = _dbContext.Accounts.SingleOrDefault(x => x.Email == email);
            if (account == null)
                return SignInErrorType.NotFound;

            var token = RandomToken(16);
            account.LoginToken = token;
            account.LoginTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15);

            var from = new EmailAddress("sendgrid@felegy.net", "Antro Radikoj: Authentication");
            var to = new EmailAddress(email);

            string contentTemplate = @"
Hello {0}, please use the following token to sign in to Antro Radikoj: {1}

This code will expire in 15 minutes.

Thank you.
";
            string content = string.Format(contentTemplate, email, token);

            var msg = MailHelper.CreateSingleEmail(from, to, "Antro Radikoj Login Code", content, content);
            var response = await _sendGridClient.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
                return SignInErrorType.SendEmailFailed;

            _dbContext.Update(account);
            await _dbContext.SaveChangesAsync();

            return null;
		}

        public async Task<SignInErrorType?> SignInAsync(HttpContext context, SignInViewModel viewModel)
        {
            var account = _dbContext.Accounts.SingleOrDefault(a => string.Equals(a.Email, viewModel.Email)); // TODO: case-friendly?
            if (account == null)
                return SignInErrorType.NotFound;

            if (viewModel.Token != account.LoginToken)
                return SignInErrorType.InvalidToken;
            if (account.LoginTokenExpiresAt < DateTimeOffset.UtcNow)
			{
                account.LoginToken = "";
                _dbContext.Update(account);
                await _dbContext.SaveChangesAsync();

                return SignInErrorType.ExpiredToken;
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

        public string RandomToken(int length)
		{
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890$&*";
            StringBuilder result = new StringBuilder();
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[length];
            rng.GetNonZeroBytes(bytes);
            for (int i = 0; i < length; i++)
			{
                result.Append(valid[bytes[i] % valid.Length]);
			}
            return result.ToString();

		}
    }
}
