using GaspApp.Data;
using GaspApp.Models;
using GaspApp.Models.ParticipateViewModels;
using GaspApp.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GaspApp.Controllers
{
    public class ParticipateController : Controller
    {
        private AzureTranslationService _translationService;
        private GaspDbContext _dbContext;

        public ParticipateController(AzureTranslationService translationService, GaspDbContext dbContext)
        {
            _translationService = translationService;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index(Guid? id = null, bool? preview = false)
        {
            var surveyResponderId = GetResponderId();

            Survey? model;
            if (id != null)
            {
                model = await _dbContext.Surveys.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
                if (model == null)
                    return NotFound();
                if (await _dbContext.SurveyResponses.Where(x => x.Survey == model).AnyAsync(x => x.ResponderId == surveyResponderId))
                {
                    if (preview == null || (preview != null && preview == false))
                        return RedirectToAction(nameof(Results), new { id = id });
                }
                if (!model.IsActive())
                    return RedirectToAction(nameof(List), routeValues: new { code = 2 });
            }
            else
                model = (await _dbContext.Surveys.Include(x => x.Items).ToListAsync())
                    .Where(x => x.IsActive())
                    .FirstOrDefault(x => !_dbContext.SurveyResponses.Any(r => r.ResponderId == surveyResponderId && r.Survey == x));

            if (model == null)
                return RedirectToAction(nameof(List), routeValues: new { code = 1 });

            if (preview != null && preview == true)
                ViewBag.Preview = true;
            return View(model);
        }

        public IActionResult SubmitResult([FromForm] IFormCollection form)
        {
            var responderId = GetResponderId();

            // Flatten form to a dictionary
            var kv = new Dictionary<string, string>();
            foreach (var (k, v) in form)
            {
                kv[k] = string.Join(";;", v);
            }
            var json = JsonConvert.SerializeObject(kv);

            var surveyId = kv["meta-survey-id"];
            var survey = _dbContext.Surveys.Find(Guid.Parse(surveyId));
            if (survey == null)
                return NotFound();

            var response = new SurveyResponse
            {
                Id = Guid.NewGuid(),
                ResponderId = responderId,
                Survey = survey,
                Country = kv["meta-country"],
                ResponseJson = json
            };
            _dbContext.SurveyResponses.Add(response);
            _dbContext.SaveChanges();

            if (kv["route"] == "another")
            {
                /* TODO another */
            }
            return RedirectToAction(nameof(Results), routeValues: new { id = survey.Id });
        }

        public async Task<IActionResult> Map()
        {
            var responses = await _dbContext.SurveyResponses.ToListAsync();

            var countByCountry = responses
                .GroupBy(x => x.Country)
                .Select(g => new { Country = g.Key, Count = g.Count() })
                .ToList();
            var totalCountries = responses.DistinctBy(x => x.Country).Count();
            var totalResponses = responses.Count();

            var viewModel = new MapViewModel
            {
                TotalCountries = totalCountries,
                TotalResponses = totalResponses,
                LocationCodes = countByCountry.Select(x => x.Country).ToList(),
                Values = countByCountry.Select(x => x.Count).ToList(),
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Results(Guid? id)
        {
            var survey = await _dbContext.Surveys.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
            if (survey == null)
                return NotFound();
            var results = await _dbContext.SurveyResponses.Where(x => x.Survey == survey).ToListAsync();
            
            var model = new ResultsViewModel();
            model.Survey = survey;
            model.TotalResponses = results.Count;
            model.UniqueCountries = results.DistinctBy(x => x.Country).Count();
            model.Questions = new List<QuestionResult>();
            foreach (var item in survey.Items.OrderBy(x => x.Position))
            {
                var questionResult = new QuestionResult
                {
                    Label = item.Label,
                    Position = item.Position,
                    IsFreeResponse = item.ItemType == SurveyItemType.FreeResponse,
                };

                if (questionResult.IsFreeResponse)
                {
                    questionResult.FreeResponses = new List<FreeResponseAnswer>();
                    foreach (var result in results)
                    {
                        var originalText = result.Response.Value<string>(item.Name)!;
                        var translatedText = await _translationService.TranslateStringAsync(
                            originalText,
                            to: Request.HttpContext.Features.Get<IRequestCultureFeature>()!.RequestCulture.Culture.TwoLetterISOLanguageName,
                            autoFrom: true); // TODO: cache me
                        var freeResponseAnswer = new FreeResponseAnswer
                        {
                            OriginalText = result.Response.Value<string>(item.Name)!,
                            TranslatedText = translatedText.Translations.Single().Text,
                        };
                        questionResult.FreeResponses.Add(freeResponseAnswer);
                    }
                }
                else
                {
                    questionResult.FixedResponses = new Dictionary<string, int>();
                    var keys = item.ParseContents();
                    foreach (var key in keys)
                        questionResult.FixedResponses[key] = 0;
                    foreach (var result in results)
                    {
                        var selections = result.Response.Value<string>(item.Name)!.Split(";;");
                        foreach (var selection in selections)
                            questionResult.FixedResponses[selection] += 1;
                    }
                }

                model.Questions.Add(questionResult);
            }
            
            return View(model);
        }

        public async Task<IActionResult> List(int? code = 0)
        {
            var responderId = GetResponderId();
            var surveys = (await _dbContext.Surveys.ToListAsync()).OrderByDescending(x => x.ActivateDate).ToList();
            var responses = await _dbContext.SurveyResponses.Where(r => r.ResponderId == responderId).ToListAsync();

            var wrappedSurveys = new List<WrappedSurvey>();
            foreach (var survey in surveys)
            {
                wrappedSurveys.Add(new WrappedSurvey
                {
                    Survey = survey,
                    HasResponded = responses.Any(r => r.Survey.Id == survey.Id)
                });
            }
            var model = new ListViewModel
            {
                Surveys = wrappedSurveys
            };
            code = code ?? 0;
            switch (code)
			{
                case 1:
                    model.Message = "You have answered all surveys currently available.";
                    break;
                case 2:
                    model.Message = "This survey is closed.";
                    break;
                default:
                    break;
            }

            return View(model);
        }

        public Guid GetResponderId()
        {
            if (!Request.Cookies.TryGetValue("AR-Survey-Responder", out var surveyResponderText))
            {
                var responderId = Guid.NewGuid();
                Response.Cookies.Append(
                    "AR-Survey-Responder",
                    responderId.ToString(),
                    new CookieOptions { IsEssential = true, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.MaxValue });
                return responderId;
            }
            else
            {
                return Guid.Parse(surveyResponderText!);
            }
        }
    }
}
