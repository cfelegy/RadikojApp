using GaspApp.Models;
using GaspApp.Models.ParticipateViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GaspApp.Controllers
{
    public class ParticipateController : Controller
    {
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
