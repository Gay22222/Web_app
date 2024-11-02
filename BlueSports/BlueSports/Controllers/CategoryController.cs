using BlueSports.Data;
using BlueSports.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlueSports.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }





        // Hiển thị danh sách Category
        public IActionResult ManageCategory()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        // Hiển thị form thêm Category mới
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View(new Category());
        }

        // Xử lý việc thêm Category
        // Xử lý việc thêm danh mục (POST)
        [HttpPost]
        public IActionResult CreateCategory(string categoryName, string description)
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
            {
                return Unauthorized();
            }

            // Kiểm tra tên danh mục đã tồn tại chưa
            var existingCategory = _context.Categories.FirstOrDefault(c => c.CategoryName == categoryName);
            if (existingCategory != null)
            {
                ViewBag.Error = "Category name already exists";
                return View(new Category { CategoryName = categoryName, Description = description });
            }

            // Tạo danh mục mới
            var newCategory = new Category
            {
                CategoryName = categoryName,
                Description = description
            };

            // Lưu vào cơ sở dữ liệu
            _context.Categories.Add(newCategory);
            _context.SaveChanges();

            // Thông báo thành công
            TempData["SuccessMessage"] = "Category added successfully.";
            return RedirectToAction("CreateCategory");
        }

        // Hiển thị form chỉnh sửa Category
        [HttpGet]
        public IActionResult EditCategory(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        public IActionResult EditCategory(int id, string categoryName, string description)
        {

            // Kiểm tra xem danh mục có tồn tại hay không
            var existingCategory = _context.Categories.FirstOrDefault(c => c.CategoryID == id);
            if (existingCategory == null)
            {
                ViewBag.Error = "Category not found";
                return View(new Category { CategoryID = id, CategoryName = categoryName, Description = description });
            }

            // Cập nhật thông tin danh mục với giá trị mới
            existingCategory.CategoryName = categoryName;
            existingCategory.Description = description;

            // Lưu vào cơ sở dữ liệu
            _context.SaveChanges();

            // Thông báo thành công
            TempData["SuccessMessage"] = "Category has been successfully updated.";
            return RedirectToAction("EditCategory"); // Chuyển hướng về trang quản lý sau khi chỉnh sửa
        }


        // Hiển thị form xác nhận xóa Category
        [HttpGet]
        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // Xử lý việc xóa Category
        [HttpPost, ActionName("DeleteCategory")]
        public IActionResult DeleteCategoryConfirmed(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.CategoryID == id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Category đã được xóa thành công.";
            }
            return RedirectToAction("ManageCategory");
        }
    }
}
