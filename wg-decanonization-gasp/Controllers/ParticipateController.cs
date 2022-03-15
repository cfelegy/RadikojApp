using GaspApp.Data;
using GaspApp.Models;
using GaspApp.Models.ParticipateViewModels;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index(Guid? id = null)
        {
            var model = new Survey
            {
                Id = Guid.NewGuid(),
                Items = new List<SurveyItem>
                {
                    new SurveyItem
                    {
                        Label = "Do you do this, that, or something else?",
                        Position = 1,
                        Name = "Item1",
                        ItemType = SurveyItemType.SingleChoice, 
                        ItemContents = "This;;That;;Something else"
                    },
                    new SurveyItem
                    {
                        Label = "Which is your favorite thing to do?",
                        Position = 2,
                        Name = "Item2",
                        ItemType = SurveyItemType.FreeResponse,
                    },
                    new SurveyItem
                    {
                        Label = "Which of the following are true?",
                        Position = 3,
                        Name = "Item 3",
                        ItemType = SurveyItemType.MultiChoice,
                        ItemContents = "1 + 1 = 2;;The sun is out at night;;This statement is false"
                    }
                }
            };
            return View(model);
        }

        public IActionResult SubmitResult([FromForm] IFormCollection form)
        {
            // Flatten form to a dictionary
            var kv = new Dictionary<string, string>();
            foreach (var (k, v) in form)
            {
                kv[k] = v.ToString(); // TODO: join multi-values using ;;
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
            return RedirectToAction("Results", routeValues: new { id = survey.Id });
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

        public IActionResult Results(Guid? id = null)
        {
            return View();
        }
    }
}
