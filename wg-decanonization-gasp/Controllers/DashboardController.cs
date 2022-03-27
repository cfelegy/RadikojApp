using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GaspApp.Data;
using GaspApp.Models.DashboardViewModels;
using GaspApp.Models;
using System.Security.Claims;
using GaspApp.Services;
using System.Linq;

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
                Surveys = surveys.Select(x =>
                {
                    return new WrappedSurvey
                    {
                        Survey = x,
                        ResponseCount = _dbContext.SurveyResponses.Count(r => r.Survey == x)
                    };
                }).ToList(),
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
        public async Task<IActionResult> ModifyArticle(Guid id, [FromForm] Article article)
        {
            var dbCopy = _dbContext.Articles.FirstOrDefault(a => a.Id == id);
            if (dbCopy == null)
                return NotFound();

            ModelState.ClearValidationState("Author");
            ModelState.ClearValidationState("Contents");
            ModelState.MarkFieldValid("Author");
            ModelState.MarkFieldValid("Contents");

            if (ModelState.IsValid)
            {
                dbCopy.Slug = article.Slug;
                dbCopy.PublishedDate = article.PublishedDate.ToUniversalTime();

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
            if (dbCopy == null)
                return NotFound();

            ModelState.ClearValidationState("Culture");
            ModelState.MarkFieldValid("Culture");

            bool autoConvert = Request.Form.TryGetValue("autoconvert", out var autoConv) && (autoConv.SingleOrDefault() ?? "false") == "true";
            if (autoConvert)
            {
                ModelState.ClearValidationState("Body");
                ModelState.MarkFieldValid("Body");
            }

            if (ModelState.IsValid)
			{
                dbCopy.Title = content.Title;
                dbCopy.Body = content.Body;
                
                if (autoConvert)
                {
                    var article = _dbContext.Articles.Include(a => a.Contents).FirstOrDefault(a => a.Contents.Any(c => c.Id == id));
                    if (article == null)
                        return NotFound("Unable to locate containing article");
                    var englishContent = article.Contents.FirstOrDefault(c => c.Culture == "en-US");
                    if (englishContent == null)
                        return NotFound("Unable to locate English content for this article, has it already been created?");
                    var translationResult = await _translationService.TranslateStringAsync(englishContent.Body, to: dbCopy.Culture);
                    var translation = translationResult.Translations.Single();
                    dbCopy.Body = translation.Text;
                }

                _dbContext.Update(dbCopy);

                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
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
                {
                    new SurveyItem
                    {
                        Id = Guid.NewGuid(),
                        Name = "test item",
                        Label = "test question",
                        ItemType = SurveyItemType.FreeResponse,
                        Position = 1,
                        ItemContents = "",
                    }
                }
            };
            _dbContext.Surveys.Add(survey);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ModifySurvey", routeValues: new { id = survey.Id });
        }

        public async Task<IActionResult> DeleteSurvey(Guid id)
        {
            // TODO: cascade deletes through to response table, this is a breaking bug
            var survey = await _dbContext.Surveys.Include(x => x.Items).FirstAsync(x => x.Id == id);
            if (survey == null)
                return NotFound();

            _dbContext.Remove(survey);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ModifySurvey(Guid id)
        {
            var survey = await _dbContext.Surveys.FindAsync(id);
            if (survey == null)
                return NotFound();
            await _dbContext.Entry(survey).Collection(x => x.Items).LoadAsync();
            if (survey.ActivateDate != null)
                survey.ActivateDate = survey.ActivateDate.Value.ToLocalTime();
            if (survey.DeactivateDate != null)
                survey.DeactivateDate = survey.DeactivateDate.Value.ToLocalTime();

            return View(survey);
        }

        [HttpPost]
        public async Task<IActionResult> ModifySurvey(Guid id, [FromForm] IFormCollection form)
        {
            var survey = await _dbContext.Surveys.FindAsync(id);
            if (survey == null)
                return NotFound();
            await _dbContext.Entry(survey).Collection(x => x.Items).LoadAsync();
            survey.Items.Clear();

            survey.Description = form["Description"].Single();

            if (form.TryGetValue("ActivateDate", out var activateDate) && activateDate.Single() != string.Empty)
                survey.ActivateDate = DateTimeOffset.Parse(activateDate.Single()).ToUniversalTime();
            if (form.TryGetValue("DeactivateDate", out var deactivateDate) && deactivateDate.Single() != string.Empty)
                survey.DeactivateDate = DateTimeOffset.Parse(deactivateDate.Single()).ToUniversalTime();

            var itemKeys = form.Keys.Where(k => k.StartsWith("si/")).Select(k => k.Split("/")[1]).Distinct();
            foreach (var itemKey in itemKeys)
            {
                SurveyItem item;
                if (itemKey.StartsWith("__GenerateGuid__"))
                    item = new SurveyItem();
                else
                {
                    if (!Guid.TryParse(itemKey, out var itemId)) {
                        return StatusCode(400);
                    }
                    var maybeItem = await _dbContext.SurveyItems.FindAsync(itemId);
                    if (maybeItem == null)
                        return NotFound();
                    item = maybeItem;
                }

                item.Name = form[$"si/{itemKey}/name"].Single();
                item.Label = form[$"si/{itemKey}/label"].Single();
                item.ItemType = (SurveyItemType)Enum.Parse(typeof(SurveyItemType), form[$"si/{itemKey}/type"].Single());
                item.Position = int.Parse(form[$"si/{itemKey}/position"].Single());
                item.ItemContents = form[$"si/{itemKey}/contents"].Single();
                
                survey.Items.Add(item);
            }

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
