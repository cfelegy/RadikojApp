using GaspApp.Models.AccountViewModels;
using GaspApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace GaspApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
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

        public IActionResult AddDebugSuperUser()
        {
            _accountService.AddDebugSuperUser();
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
