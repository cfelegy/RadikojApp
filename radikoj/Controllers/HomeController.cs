using System.Diagnostics;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Radikoj.Data;
using Radikoj.Models;

namespace Radikoj.Controllers
{
    public class HomeController : Controller
    {
        private RadikojDbContext _dbContext;

        public HomeController(RadikojDbContext dbContext)
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
                if ((content = featureArticle.Contents.FirstOrDefault(x => string.Equals(x.Culture, cultureName, StringComparison.OrdinalIgnoreCase))) != null)
                    ViewBag.ArticleContent = content;
                else
                    ViewBag.ArticleContent = featureArticle.Contents.FirstOrDefault()!;
                ViewBag.MostRecentArticle = featureArticle;
            }

            var homeArticle = await _dbContext.Articles.Include(a => a.Contents)
                .FirstOrDefaultAsync(a => a.Slug == "[home]");
            if (homeArticle == null)
            {
                homeArticle = new Article
                {
                    Slug = "[home]",
                    PublishedDate = DateTimeOffset.MaxValue,
                    Author = null,
                    Contents = new List<ArticleContent>(),
                    Id = Guid.NewGuid(),
                };
                _dbContext.Articles.Add(homeArticle);
                await _dbContext.SaveChangesAsync();
            }

            ArticleContent? homeContent;
            if ((homeContent = homeArticle.Contents.FirstOrDefault(x => string.Equals(x.Culture, cultureName, StringComparison.OrdinalIgnoreCase))) != null)
                ViewBag.HomeContent = homeContent;
            else
                ViewBag.HomeContent = homeArticle.Contents.FirstOrDefault()!;

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
