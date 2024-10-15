using Microsoft.AspNetCore.Mvc;

namespace Blazor_Test_App.Controllers
{
    public class HomeController : Controller
    {
        // Action cho trang chủ
        public IActionResult Index()
        {
            return View();
        }

        // Action cho trang lỗi (optional)
        public IActionResult Error()
        {
            return View();
        }
    }
}
