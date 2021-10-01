using Microsoft.AspNetCore.Mvc;

namespace GaspApp.Controllers
{
    public class SurveyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
