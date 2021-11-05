using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GaspApp.Data;
using GaspApp.Models.DashboardViewModels;
using GaspApp.Models;
using System.Security.Claims;

namespace GaspApp.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private GaspDbContext _dbContext;

        public DashboardController(GaspDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var articles = _dbContext.Articles.Include(x => x.Contents).ToList();
            var model = new DashboardIndexViewModel
            {
                Articles = articles,
            };
            return View(model);
        }

        public async Task<IActionResult> CreateArticle()
        {
            var account = await _dbContext.Accounts.FindAsync(new Guid(User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)!.Value));
            var article = new Article
            {
                Author = account,
                Slug = string.Format("new-article-{0}", Guid.NewGuid().ToString()),
                Contents = new List<ArticleContent> { new ArticleContent { Culture = "en-US", Title = "new-content", Body = "" } },
            };
            _dbContext.Articles.Add(article);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ModifyArticle", routeValues: new { id = article.Id });
        }

        public async Task<IActionResult> DeleteArticle(Guid id)
		{
            // TODO: double-check with user
            var article = await _dbContext.Articles.FindAsync(id);
            if (article == null)
                return NotFound();

            await _dbContext.Entry(article).Collection(x => x.Contents).LoadAsync();
            article.Contents.RemoveAll(x => true);

            _dbContext.Articles.Remove(article);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
		}

        public async Task<IActionResult> ModifyArticle(Guid id)
        {
            var article = await _dbContext.Articles.FindAsync(id);
            if (article == null)
                return NotFound();
            await _dbContext.Entry(article).Collection(x => x.Contents).LoadAsync();
            await _dbContext.Entry(article).Reference(x => x.Author).LoadAsync();

            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifyArticle(Guid id, [Bind("Slug")] Article article)
        {
            var dbCopy = _dbContext.Articles.FirstOrDefault(a => a.Id == id);

            ModelState.ClearValidationState("Author");
            ModelState.ClearValidationState("Contents");
            ModelState.MarkFieldValid("Author");
            ModelState.MarkFieldValid("Contents");

            if (ModelState.IsValid)
            {
                dbCopy.Slug = article.Slug;

                _dbContext.Update(dbCopy);

                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dbCopy);
        }

        public async Task<IActionResult> CreateContent(Guid articleId, string locale)
		{
            if (!LocaleConstants.SUPPORTED_LOCALES.Contains(locale))
                return BadRequest("locale not supported");
            
            var article = await _dbContext.Articles.FindAsync(articleId);
            if (article == null)
                return NotFound("article");
            
            await _dbContext.Entry(article).Collection(x => x.Contents).LoadAsync();
            if (article.Contents.Any(x => x.Culture == locale))
                return BadRequest("locale already exists");

            var content = new ArticleContent
            {
                Culture = locale,
                Title = $"{article.Slug}, {locale}",
                Body = "",
            };
            article.Contents.Add(content);
            _dbContext.Update(article);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(ModifyContent), routeValues: new { articleId = article.Id, contentId = content.Id });
		}

        public async Task<IActionResult> ModifyContent(Guid articleId, Guid contentId)
		{
            var article = await _dbContext.Articles.FindAsync(articleId);
            if (article == null)
                return NotFound();
            await _dbContext.Entry(article).Collection(x => x.Contents).LoadAsync();
            var content = article.Contents.FirstOrDefault(x => x.Id == contentId);
            if (content == null)
                return NotFound();
            var viewModel = new DashboardModifyContentViewModel
            {
                Parent = article,
                Content = content,
            };
            return View(content);
		}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifyContent(Guid id, [Bind("Title", "Body")] ArticleContent content)
		{
            var dbCopy = _dbContext.Articles.Include(x => x.Contents).SelectMany(x => x.Contents).FirstOrDefault(c => c.Id == id);

            ModelState.ClearValidationState("Culture");
            ModelState.MarkFieldValid("Culture");

            if (ModelState.IsValid)
			{
                dbCopy.Title = content.Title;
                dbCopy.Body = content.Body;

                _dbContext.Update(dbCopy);

                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
			}
            return View(dbCopy);
		}

        public IActionResult Translations()
		{
            return View();
		}
    }
}
