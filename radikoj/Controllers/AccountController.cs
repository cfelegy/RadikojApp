using GaspApp.Models.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Radikoj.Data;
using Radikoj.Models;
using Radikoj.Models.AccountViewModels;
using Radikoj.Services;
using System.Security.Claims;

namespace Radikoj.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;
        private readonly RadikojDbContext _dbContext;

        public AccountController(AccountService accountService, RadikojDbContext dbContext)
        {
            _accountService = accountService;
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult SignIn(string? returnUrl = "")
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        public async Task<IActionResult> SignIn(SignInViewModel viewModel, string? returnUrl = "")
        {
            returnUrl = returnUrl ?? "~/Dashboard";

            IActionResult Fail()
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(viewModel);
            }

            if (!ModelState.IsValid)
                return Fail();

            if (string.IsNullOrEmpty(viewModel.Token))
			{
                var result = await _accountService.CreateSignInToken(viewModel.Email);
                if (result.HasValue)
                {
                    ModelState.AddModelError("Sign in Failure", result.ToString()!);
                }
                else
                {
                    ModelState.AddModelError("In progress:", "A temporary token has been emailed to you. Please check your email and use the token to complete sign-in");
                }
                return Fail();
			}
            else
			{
                var result = await _accountService.SignInAsync(HttpContext, viewModel);
                if (result != null)
				{
                    ModelState.AddModelError("Sign in Failure", result.ToString()!);
                    return Fail();
                }
				else
				{
                    return ReturnUrlAction(returnUrl);
                }
			}
        }

        public async Task<IActionResult> SignOut(string? returnUrl = "")
        {
            await _accountService.SignOutAsync(HttpContext);

            return ReturnUrlAction(returnUrl);
        }

        [Authorize]
        public IActionResult ListAll()
		{
            var accounts = _dbContext.Accounts.ToList();
            return View(accounts);
		}

        [Authorize]
        public async Task<IActionResult> AlterAccountState(Guid id, string state)
        {
            var currentAccount = await _dbContext.Accounts.FindAsync(new Guid(User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)!.Value));
            if (!currentAccount!.SuperUser) return Forbid();

            if (!(state == "enable" || state == "disable"))
                return NotFound();
            var account = await _dbContext.Accounts.FindAsync(id);
            if (account == null)
                return NotFound();

            var stateBool = state == "disable";
            account.Disabled = stateBool;

            _dbContext.Update(account);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(ListAll));
        }
        
        [Authorize]
        public async Task<IActionResult> AddUser()
        {
            var account = await _dbContext.Accounts.FindAsync(new Guid(User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)!.Value));
            if (!account!.SuperUser) return Forbid();

            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddUser([FromForm] CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            
            var account = await _dbContext.Accounts.FindAsync(new Guid(User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)!.Value));
            if (!account!.SuperUser) return Forbid();

            _dbContext.Accounts.Add(new Account
            {
                Id = Guid.NewGuid(),
                DisplayName = model.Name,
                Email = model.Email.ToLower(),
                Disabled = false,
                LoginToken = "X",
                LoginTokenExpiresAt = DateTimeOffset.MinValue,
                SuperUser = false
            });
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(ListAll));
        }
        [Authorize]
        public async Task<IActionResult> ModifyUser(Guid id)
        {
            var currentAccount = await _dbContext.Accounts.FindAsync(new Guid(User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)!.Value));
            if (!currentAccount!.SuperUser) return Forbid();

            var account = await _dbContext.Accounts.FindAsync(id);
            if (account == null) return NotFound("account id");

            var model = new ModifyUserViewModel
            {
                Id = id,
                Name = account.DisplayName,
                Email = account.Email,
                SuperUser = account.SuperUser
            };

            return View(model);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ModifyUser([FromForm] ModifyUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var currentAccount = await _dbContext.Accounts.FindAsync(new Guid(User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)!.Value));
            if (!currentAccount!.SuperUser) return Forbid();

            var account = await _dbContext.Accounts.FindAsync(model.Id);
            if (account == null) return NotFound("account id");

            account.DisplayName = model.Name;
            account.Email = model.Email.ToLower();
            account.SuperUser = model.SuperUser;

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(ListAll));
        }

        private IActionResult ReturnUrlAction(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction("Index", "Home");
        }
    }
}
