using GaspApp.Data;
using GaspApp.Models;
using GaspApp.Models.ParticipateViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GaspApp.Controllers
{
    public class ParticipateController : Controller
    {
        private GaspDbContext _dbContext;

        public ParticipateController(GaspDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index(Guid? id = null)
        {
            Survey? model;
            if (id != null)
            {
                model = await _dbContext.Surveys.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
                if (model == null)
                    return NotFound();
            }
            else
                model = (await _dbContext.Surveys.Include(x => x.Items).ToListAsync()).FirstOrDefault(x => x.IsActive());

            if (model == null)
                return RedirectToAction(nameof(NoActiveSurveys));

            return View(model);
        }

        public IActionResult NoActiveSurveys() => View();

        public IActionResult SubmitResult([FromForm] IFormCollection form)
        {
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
                        var freeResponseAnswer = new FreeResponseAnswer
                        {
                            OriginalText = result.Response.Value<string>(item.Name)!,
                            // TODO: TranslatedText
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
    }
}
