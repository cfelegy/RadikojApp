using GaspApp.Data;
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
            IActionResult Fail()
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(viewModel);
            }

            if (!ModelState.IsValid)
                return Fail();

            var signInFailure = await _accountService.SignInAsync(HttpContext, viewModel);
            if (signInFailure != null)
            {
                ModelState.AddModelError("Sign in Failure", signInFailure.ToString()!);
                return Fail();
            }

            return ReturnUrlAction(returnUrl);
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

        public IActionResult AddDebugSuperUser()
        {
            _accountService.AddDebugSuperUser();
            return RedirectToAction("SignIn");
        }
        public IActionResult AddDebugDemoUser()
        {
            _accountService.AddDemoUser();
            return RedirectToAction("SignIn");
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
