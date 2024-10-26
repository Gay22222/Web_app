using Microsoft.AspNetCore.Mvc;

namespace BlueSports.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

}
