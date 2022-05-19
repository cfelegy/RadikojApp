using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Radikoj.Data;
using Radikoj.Models;
using Radikoj.Models.ArticlesViewModels;

namespace Radikoj.Controllers
{
    public class ArticlesController : Controller
    {
        private RadikojDbContext _dbContext;

        public ArticlesController(RadikojDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var articles = (await _dbContext.Articles.Include(x => x.Contents).ToListAsync()).OrderByDescending(x => x.PublishedDate).Where(x => x.IsPublished()).ToList();
            var model = new ArticlesIndexViewModel
            {
                Articles = articles
            };

            return View(model);
        }

        [Route("[controller]/{slug}")]
        public IActionResult Article(string slug)
        {
            var article = _dbContext.Articles.SingleOrDefault(x => x.Slug == slug);
            if (article == null)
            {
                return NotFound();
            }
            _dbContext.Entry(article).Collection(x => x.Contents).Load();
            _dbContext.Entry(article).Reference(x => x.Author).Load();

            var cultureFeature = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var cultureName = cultureFeature!.RequestCulture.Culture.Name;
            
            var viewModel = new ArticlesArticleViewModel
            {
                Article = article
            };
            ArticleContent? content;
            if ((content = article.Contents.FirstOrDefault(x => string.Equals(x.Culture, cultureName, StringComparison.OrdinalIgnoreCase))) != null)
                viewModel.Content = content;
			else
			{
                viewModel.Content = article.Contents.First();
                viewModel.Fallback = true; // TODO: show fallback
			}
            
            return View(viewModel);
        }
    }
}
