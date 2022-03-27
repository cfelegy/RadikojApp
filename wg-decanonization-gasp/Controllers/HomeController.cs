using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GaspApp.Models;
using Microsoft.AspNetCore.Localization;
using GaspApp.Data;
using Microsoft.EntityFrameworkCore;

namespace GaspApp.Controllers
{
    public class HomeController : Controller
    {
        private GaspDbContext _dbContext;

        public HomeController(GaspDbContext dbContext)
		{
            _dbContext = dbContext;
		}

        public async Task<IActionResult> Index()
        {
            var cultureFeature = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var cultureName = cultureFeature!.RequestCulture.Culture.Name;

            var featureArticle = (await _dbContext.Articles.Include(a => a.Contents).OrderByDescending(x => x.PublishedDate).ToListAsync())
                .Where(a => a.IsPublished()).FirstOrDefault();
            if (featureArticle != null)
			{
                ArticleContent? content;
                if ((content = featureArticle.Contents.FirstOrDefault(x => x.Culture == cultureName)) != null)
                    ViewBag.ArticleContent = content;
                else
                    ViewBag.ArticleContent = featureArticle.Contents.FirstOrDefault();
                ViewBag.MostRecentArticle = featureArticle;
            }

            return View();
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
