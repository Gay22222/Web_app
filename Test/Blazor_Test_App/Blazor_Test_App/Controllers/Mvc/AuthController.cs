using Microsoft.AspNetCore.Mvc;

namespace Blazor_Test_App.Controllers
{
    public class AuthController : Controller
    {
        // Trang Đăng nhập
        public IActionResult Login()
        {
            return View();
        }

        // Trang Đăng ký
        public IActionResult Register()
        {
            return View();
        }

        // Xử lý Đăng nhập
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Logic đăng nhập ở đây (kiểm tra username và password)
            if (username == "admin" && password == "password")
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Invalid credentials";
            return View();
        }

        // Xử lý Đăng ký
        [HttpPost]
        public IActionResult Register(string username, string password)
        {
            // Logic đăng ký ở đây
            // Lưu thông tin người dùng vào database (giả sử)
            return RedirectToAction("Login");
        }
    }
}
