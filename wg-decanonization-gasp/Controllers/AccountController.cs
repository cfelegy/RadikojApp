using GaspApp.Data;
using GaspApp.Models;
using GaspApp.Models.AccountViewModels;
using GaspApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaspApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;
        private readonly GaspDbContext _dbContext;

        public AccountController(AccountService accountService, GaspDbContext dbContext)
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

        [Authorize] // TODO: require superuser
        public async Task<IActionResult> AlterAccountState(Guid id, string state)
        {
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

        private IActionResult ReturnUrlAction(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction("Index", "Home");
        }
    }
}
