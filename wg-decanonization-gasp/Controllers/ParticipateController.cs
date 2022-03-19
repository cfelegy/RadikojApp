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
                model = (await _dbContext.Surveys.ToListAsync()).FirstOrDefault(x => x.IsActive());

            if (model == null)
                return NoActiveSurveys();

            return View(model);
        }

        public IActionResult NoActiveSurveys() => View();

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
