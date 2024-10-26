using BlueSports.Data;
using BlueSports.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BlueSports.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang đăng ký
        [HttpGet]
        [AllowAnonymous]
        [Route("Register", Name = "DangKy")]
        public IActionResult Register()
        {
            return View();
        }

        // Xử lý đăng ký

        [HttpPost]
        [AllowAnonymous]
        [Route("Register", Name = "DangKy")]
        public IActionResult Register(string username, string email, string password, string confirmPassword,string phoneNumber, string userType = "Customer")
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                return View();
            }

            // Kiểm tra xem tên người dùng hoặc email đã tồn tại chưa
            var existingUser = _context.Users.FirstOrDefault(u => u.UserName == username || u.Email == email);
            if (existingUser != null)
            {
                ViewBag.Error = "Username or Email already exists";
                return View();
            }

            // Mã hóa mật khẩu
            string hashedPassword = HashPassword(password);

            // Tạo người dùng mới
            var newUser = new User
            {
                UserName = username,
                Email = email,
                PasswordHash = hashedPassword,
                PhoneNumber = phoneNumber,
                UserType = userType, // Tùy thuộc vào việc đăng ký là Admin hay Customer
                DateJoined = DateTime.Now,
                ShippingAddress = "", // Bạn có thể thay đổi giá trị mặc định này
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();


            return RedirectToAction("Login");
        }

        // Trang đăng nhập
        [AllowAnonymous]
        [Route("Log-in", Name = "DangNhap")]
        public IActionResult Login()
        {
            return View();
        }

        // Xử lý đăng nhập
        [HttpPost]
        [AllowAnonymous]
        [Route("Log-in", Name = "DangNhap")]
        public IActionResult Login(string email, string password)
        {
            // Tìm người dùng bằng email
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid email or password";
                return View();
            }

            // Lưu thông tin UserType vào Session sau khi đăng nhập thành công
            HttpContext.Session.SetString("UserType", user.UserType);
            HttpContext.Session.SetInt32("UserID", user.UserID);

            // Điều hướng đến trang chủ
            return RedirectToAction("Index", "Home");
        }

        // Hàm mã hóa mật khẩu
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // Hàm kiểm tra mật khẩu
        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }

        [Route("my-account", Name = "Dashboard")]
        public IActionResult Dashboard()
        {
            var taikhoanID = HttpContext.Session.GetString("UserId");
            if (taikhoanID != null)
            {
                var khachhang = _context.Users.SingleOrDefault(u => u.UserID == Convert.ToInt32(taikhoanID));
                if (khachhang != null)
                {
                    var lsDonHang = _context.Orders
                        //.Include(x => x.ShippingStatus)
                        .Where(x => x.UserID == khachhang.UserID)
                        .OrderByDescending(x => x.OrderDate)
                        .ToList();
                    ViewBag.DonHang = lsDonHang;
                    return View(khachhang);
                }
            }
            return RedirectToAction("Login");
        }

    }
}
