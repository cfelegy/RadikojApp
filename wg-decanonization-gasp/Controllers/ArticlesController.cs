using GaspApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace GaspApp.Controllers
{
    public class ArticlesController : Controller
    {
        private GaspDbContext _dbContext;

        public ArticlesController(GaspDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var articles = _dbContext.Articles.ToList();
            var model = new Models.ArticlesViewModels.ArticlesIndexViewModel
            {
                Articles = articles
            };

            return View(model);
        }
    }
}
