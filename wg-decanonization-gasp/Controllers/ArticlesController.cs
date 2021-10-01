using Microsoft.AspNetCore.Mvc;

namespace GaspApp.Controllers
{
    public class ArticlesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
