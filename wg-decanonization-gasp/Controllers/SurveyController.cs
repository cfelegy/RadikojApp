using GaspApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace GaspApp.Controllers
{
    public class SurveyController : Controller
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
                        Label = "Placeholder for dynamic Item 1",
                        Position = 1,
                        Name = "Item1"
                    },
                    new SurveyItem
                    {
                        Label = "Placeholder for a second dynamic item",
                        Position = 2,
                        Name = "Item2"
                    }
                }
            };
            return View(model);
        }
    }
}
