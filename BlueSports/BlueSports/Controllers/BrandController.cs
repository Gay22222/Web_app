using BlueSports.Data;
using BlueSports.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlueSports.Controllers
{
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BrandController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách Brand
        public IActionResult ManageBrand()
        {
            var brands = _context.Brands.ToList();
            return View(brands);
        }

        // Hiển thị form thêm Brand mới
        [HttpGet]
        public IActionResult CreateBrand()
        {
            return View(new Brand());
        }

        // Xử lý việc thêm thương hiệu (POST)
        [HttpPost]
        public IActionResult CreateBrand(string brandName, string description)
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
            {
                return Unauthorized();
            }

            // Kiểm tra tên thương hiệu đã tồn tại chưa
            var existingBrand = _context.Brands.FirstOrDefault(b => b.BrandName == brandName);
            if (existingBrand != null)
            {
                ViewBag.Error = "Brand name already exists";
                return View(new Brand { BrandName = brandName, Description = description });
            }

            // Tạo thương hiệu mới
            var newBrand = new Brand
            {
                BrandName = brandName,
                Description = description
            };

            // Lưu vào cơ sở dữ liệu
            _context.Brands.Add(newBrand);
            _context.SaveChanges();

            // Thông báo thành công
            TempData["SuccessMessage"] = "Brand added successfully.";
            return RedirectToAction("CreateBrand");
        }

        // Hiển thị form chỉnh sửa Brand
        [HttpGet]
        public IActionResult EditBrand(int id)
        {
            var brand = _context.Brands.FirstOrDefault(b => b.BrandID == id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        // Xử lý việc chỉnh sửa Brand
        [HttpPost]
        public IActionResult EditBrand(int id, string brandName, string description)
        {
            // Kiểm tra quyền Admin
            if (HttpContext.Session.GetString("UserType") != "Admin")
            {
                return Unauthorized();
            }

            // Lấy brand từ cơ sở dữ liệu theo ID
            var existingBrand = _context.Brands.FirstOrDefault(b => b.BrandID == id);
            if (existingBrand == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy brand
            }

            // Cập nhật thông tin brand
            existingBrand.BrandName = brandName;
            existingBrand.Description = description;

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.Brands.Update(existingBrand);
            _context.SaveChanges();

            // Thông báo thành công
            TempData["SuccessMessage"] = "Brand has been updated successfully.";

            return RedirectToAction("EditBrand"); // Chuyển hướng về trang quản lý Brand
        }

        // Hiển thị form xác nhận xóa Brand
        [HttpGet]
        public IActionResult DeleteBrand(int id)
        {
            var brand = _context.Brands.FirstOrDefault(b => b.BrandID == id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        // Xử lý việc xóa Brand
        [HttpPost, ActionName("DeleteBrand")]
        public IActionResult DeleteConfirmed(int id)
        {
            var brand = _context.Brands.FirstOrDefault(b => b.BrandID == id);
            if (brand != null)
            {
                _context.Brands.Remove(brand);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Brand đã được xóa thành công.";
            }
            return RedirectToAction("ManageBrand");
        }
    }
}
