using AspNetCoreHero.ToastNotification.Abstractions;
using BlueSports.Data;
using BlueSports.HandleAdmin.ModelViews;
using BlueSports.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BlueSports.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly INotyfService _notyfService;
        public AccountController(ApplicationDbContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
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
        public async Task<IActionResult> Register(RegisterViewModel newUser, string userType = "Customer")
        {
            // Kiểm tra xem tên người dùng hoặc email đã tồn tại chưa
            var existingUser = _context.Users.FirstOrDefault(u => u.UserName == newUser.UserName || u.Email == newUser.Email);
            if (existingUser != null)
            {
                ViewBag.Error = "Username or Email already exists";
                return View();
            }

            // Mã hóa mật khẩu
            string hashedPassword = HashPassword(newUser.Password);
            User Newuser = new User
            {
                UserName = newUser.UserName,
                Email = newUser.Email, // Dùng đúng Email từ newUser
                PasswordHash = hashedPassword,
                PhoneNumber = newUser.PhoneNumber,
                UserType = userType, // Tùy thuộc vào việc đăng ký là Admin hay Customer
                DateJoined = DateTime.Now,
                ShippingAddress = "", // Bạn có thể thay đổi giá trị mặc định này
            };

            try
            {
                _context.Users.Add(Newuser);
                await _context.SaveChangesAsync();

                // Tạo claims và đăng nhập người dùng ngay lập tức sau khi đăng ký
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, Newuser.Email),
                    new Claim("UserId", Newuser.UserID.ToString()),
                    new Claim("UserType", Newuser.UserType) // Thêm UserType nếu cần
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                _notyfService.Success("Đăng ký thành công");
                // Điều hướng đến trang Dashboard sau khi đăng ký thành công
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                ViewBag.Error = "Registration failed. Please try again.";
                return View();
            }
        }


        // Trang đăng nhập
        [AllowAnonymous]
        [Route("Log-in", Name = "DangNhap")]
        public IActionResult Login()
        {
            var userID = HttpContext.Session.GetString("UserId");
            if (userID != null)
            {
                return RedirectToAction("Dashboard", "Account");
            }
            return View();
        }

        // Xử lý đăng nhập
        [HttpPost]
        [AllowAnonymous]
        [Route("Log-in", Name = "DangNhap")]
        public async Task<IActionResult> Login(LoginViewModel userLogin)
        {
            // Tìm người dùng bằng email
            var user = _context.Users.FirstOrDefault(u => u.Email == userLogin.Email);
            if (user == null || !VerifyPassword(userLogin.Password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid email or password";
                return View();
            }
            HttpContext.Session.SetString("UserType", user.UserType);
            HttpContext.Session.SetString("UserID", user.UserID.ToString());
            var userID = HttpContext.Session.GetString("UserID");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("UserId", user.UserID.ToString()),
                new Claim("UserType", user.UserType) // Thêm UserType nếu cần
            };

            // Tạo ClaimsIdentity với AuthenticationScheme mặc định
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Đăng nhập người dùng và thiết lập cookie xác thực
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            _notyfService.Success("Đăng nhập thành công");
            // Điều hướng đến trang chủ sau khi đăng nhập thành công
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
            if (User.Identity.IsAuthenticated)
            {
                var userID = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                if (!string.IsNullOrEmpty(userID))
                {
                    var user = _context.Users.FirstOrDefault(u => u.UserID == Convert.ToInt32(userID));
                    if (user != null)
                    {
                        var lsDonHang = _context.Orders
                            .Where(x => x.UserID == user.UserID)
                            .OrderByDescending(x => x.OrderDate)
                            .ToList();
                        ViewBag.DonHang = lsDonHang;
                        return View(user);
                    }
                }
            }
            return RedirectToAction("Login");
        }


        [HttpGet]
        [Route("Logout", Name = "DangXuat")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("UserId");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize] // Chỉ cho phép người dùng đã đăng nhập
        [Route("ChangePassword", Name = "ThayDoiMatKhau")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Invalid input data.";
                return View(model);
            }

            // Lấy UserID từ claims để xác định người dùng hiện tại
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            // Tìm người dùng trong cơ sở dữ liệu
            var user = _context.Users.SingleOrDefault(u => u.UserID == Convert.ToInt32(userId));
            if (user == null)
            {
                ViewBag.Error = "User not found.";
                return View(model);
            }

            // Xác minh mật khẩu hiện tại (PasswordNow)
            if (!VerifyPassword(model.PasswordNow, user.PasswordHash))
            {
                ViewBag.Error = "Current password is incorrect.";
                return View(model);
            }

            // Xác minh mật khẩu mới (Password) và mật khẩu xác nhận (ConfirmPassword) trùng khớp
            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "New password and confirmation do not match.";
                return View(model);
            }

            // Mã hóa mật khẩu mới
            user.PasswordHash = HashPassword(model.Password);

            // Cập nhật mật khẩu trong cơ sở dữ liệu
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                ViewBag.Success = "Password changed successfully.";
            }
            catch
            {
                ViewBag.Error = "Failed to change password. Please try again.";
                return View(model);
            }
            _notyfService.Success("Thay đổi mật khẩu thành công");
            return RedirectToAction("Dashboard", "Account");
        }

    }


}
