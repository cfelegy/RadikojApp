using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GaspApp.Models;
using Microsoft.AspNetCore.Localization;
using GaspApp.Models.HomeViewModels;

namespace GaspApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Map()
		{
            var viewModel = new MapViewModel
            {
                LocationCodes = "['USA', 'GBR']",
                LocationNames = "['United States', 'United Kingdom']",
                Values = "[10, 15]"
            };
            return View(viewModel);
		}

        //[HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        public IActionResult Error(int? statusCode = null)
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, ErrorCode = statusCode });
        }
    }
}
