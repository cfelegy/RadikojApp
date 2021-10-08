using GaspApp.Data;
using GaspApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GaspApp.Controllers
{
    public class ArticlesController : Controller
    {
        private GaspDbContext _dbContext;

        public ArticlesController(GaspDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var articles = _dbContext.Articles.Include(x => x.Contents).ToList();
            var model = new Models.ArticlesViewModels.ArticlesIndexViewModel
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
            
            // TODO: ArticlesArticleViewModel to localize body; just debugging for now
            return View(article);
        }

        [Authorize]
        public async Task<IActionResult> AddDebugArticles()
        {
            var account = await _dbContext.Accounts.FindAsync(new Guid(User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)!.Value));
            var contentEn = new ArticleContent
            {
                Culture = "en-US", // hard-coding like this is bad
                Title = "Test Article 1",
                Body = "Contents written for test article 1..."
            };
            var contentEs = new ArticleContent
            {
                Culture = "es",
                Title = "Aritculo de Prueba 1",
                Body = "Los contenidos para el articulo de prueba 1..."
            };
            var article = new Article
            {
                Slug = "test-1",
                Author = account,
                Contents = new List<ArticleContent> { contentEn, contentEs },
                PublishedDate = DateTimeOffset.Now.ToUniversalTime(),
            };
            await _dbContext.Articles.AddAsync(article);
            await _dbContext.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }
    }
}
