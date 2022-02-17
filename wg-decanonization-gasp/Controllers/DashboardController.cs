using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GaspApp.Data;
using GaspApp.Models.DashboardViewModels;
using GaspApp.Models;
using System.Security.Claims;
using GaspApp.Services;

namespace GaspApp.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private GaspDbContext _dbContext;
        private AzureTranslationService _translationService;

        public DashboardController(GaspDbContext dbContext, AzureTranslationService translationService)
        {
            _dbContext = dbContext;
            _translationService = translationService;
        }

        public IActionResult Index()
        {
            var articles = _dbContext.Articles.Include(x => x.Contents).ToList();
            var surveys = _dbContext.Surveys.ToList();
            var model = new DashboardIndexViewModel
            {
                Articles = articles,
                Surveys = surveys,
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

        public async Task<IActionResult> CreateSurvey()
        {
            var survey = new Survey
            {
                Id = Guid.NewGuid(),
                Description = string.Empty,
                Items = new List<SurveyItem>()
            };
            _dbContext.Surveys.Add(survey);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ModifySurvey", routeValues: new { id = survey.Id });
        }

        public async Task<IActionResult> DeleteSurvey(Guid id)
        {
            // TODO: double-check with user
            var survey = await _dbContext.Surveys.FindAsync(id);
            if (survey == null)
                return NotFound();

            _dbContext.Surveys.Remove(survey);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ModifySurvey(Guid id)
        {
            var survey = await _dbContext.Surveys.FindAsync(id);
            if (survey == null)
                return NotFound();
            await _dbContext.Entry(survey).Collection(x => x.Items).LoadAsync();

            return View(survey);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifySurvey(Guid id, Survey survey)
        {
            if (id != survey.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _dbContext.Update(survey);

                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(survey);
        }

        public IActionResult Translations()
		{
            var translations = _dbContext.LocalizedItems.ToList().GroupBy(i => i.Key).Select(
                x =>
                {
                    return new TranslationsLocalizedItem
                    {
                        Key = x.Key,
                        Values = x.ToDictionary(k => k.CultureCode, v => v.Text)
                    };
                }
            );
            var model = new TranslationsViewModel
            {
                Items = translations.ToList(),
            };
            return View(model);
		}

        public IActionResult ModifyTranslation([FromBody] TranslationsLocalizedItem model)
        {
            return Json(ModifyTranslationInternal(model));
        }

        public async Task<IActionResult> AutoTranslation([FromBody] TranslationsLocalizedItem model)
        {
            var key = model.Key;
            var english = model.Values["en"];
            var translation = await _translationService.TranslateStringAsync(english);

            var localizedItem = new TranslationsLocalizedItem
            {
                Key = key,
                Values = new Dictionary<string, string>()
            };
            localizedItem.Values["en"] = english;
            foreach (var translatedItem in translation.Translations)
                localizedItem.Values[translatedItem.To] = translatedItem.Text;
            localizedItem.Values["zh"] = localizedItem.Values["zh-Hans"];
            localizedItem.Values.Remove("zh-Hans");

            return Json(ModifyTranslationInternal(localizedItem));
        }

        private TranslationsLocalizedItem ModifyTranslationInternal(TranslationsLocalizedItem model)
        {
            var items = _dbContext.LocalizedItems.Where(x => x.Key == model.Key);
            foreach (var item in items)
            {
                if (item.Text != model.Values[item.CultureCode])
                    item.Text = model.Values[item.CultureCode];
            }
            _dbContext.SaveChanges();

            // rebuild model based on database
            return new TranslationsLocalizedItem
            {
                Key = items.First().Key,
                Values = items.ToDictionary(k => k.CultureCode, v => v.Text)
            };
        }
    }
}
