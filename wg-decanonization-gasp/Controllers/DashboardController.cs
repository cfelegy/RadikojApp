using Microsoft.AspNetCore.Mvc;

namespace GaspApp.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
