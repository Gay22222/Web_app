using Microsoft.AspNetCore.Mvc;
using Blazor_Test_App.Models;
using System.Collections.Generic;
using Blazor_Test_App.Data;

namespace Blazor_Test_App.Controllers
{
    public class ProductController : Controller
    {
        // Danh sách sản phẩm mẫu
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách sản phẩm từ CSDL
        public IActionResult ListProduct()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        // Hiển thị form thêm sản phẩm (Chỉ dành cho Admin)
        [HttpGet]
        public IActionResult CreateProduct()
        {
            // Kiểm tra quyền Admin
            if (HttpContext.Session.GetString("UserType") != "Admin")
            {
                return Unauthorized();
            }
            // Lấy danh sách Brand và Category từ CSDL
            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.Categories = _context.Categories.ToList();
            return View(new Product()); // Hiển thị form thêm sản phẩm
        }
        // Xử lý việc thêm sản phẩm (POST)
        [HttpPost]
        [HttpPost]
        public IActionResult CreateProduct(string productName, decimal price, int stockQuantity, string description, string imageUrl, int brandId, int categoryId)
        {
            // Kiểm tra quyền Admin
            if (HttpContext.Session.GetString("UserType") != "Admin")
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                // Tạo sản phẩm mới với các thông tin được nhận từ form
                var newProduct = new Product
                {
                    ProductName = productName,
                    Price = price,
                    StockQuantity = stockQuantity,
                    Description = description,
                    ImageURL = imageUrl,
                    DateAdded = DateTime.Now, // Ngày tạo sản phẩm
                    LastUpdated = DateTime.Now, // Ngày cập nhật sản phẩm
                    BrandID = brandId, // Gán BrandID từ form (DropdownList)
                    CategoryID = categoryId // Gán CategoryID từ form (DropdownList)
                };

                // Lưu sản phẩm vào cơ sở dữ liệu
                _context.Products.Add(newProduct);
                _context.SaveChanges();

                // Thông báo thành công
                TempData["SuccessMessage"] = "Sản phẩm đã được thêm thành công.";
                return RedirectToAction("CreateProduct");
            }

            // Nếu dữ liệu không hợp lệ, trả về view với thông báo lỗi
            return View(new Product());
        }

        // Hiển thị chi tiết sản phẩm
        public IActionResult Details(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public IActionResult ManageProducts()
        {
            var products = _context.Products.ToList();
            return View(products); // Hiển thị danh sách sản phẩm hoặc các chức năng quản lý sản phẩm
        }


        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            // Lấy sản phẩm từ cơ sở dữ liệu theo ID
            var product = _context.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return NotFound(); // Nếu không tìm thấy sản phẩm, trả về 404
            }

            return View(product); // Trả về form chỉnh sửa với dữ liệu sản phẩm
        }

        // Xử lý khi người dùng submit form để sửa sản phẩm
        // Xử lý việc chỉnh sửa sản phẩm
        [HttpPost]
        public IActionResult EditProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                // Tìm sản phẩm từ cơ sở dữ liệu
                var existingProduct = _context.Products.FirstOrDefault(p => p.ProductID == product.ProductID);

                if (existingProduct != null)
                {
                    // Cập nhật thông tin sản phẩm
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.BrandID = product.BrandID;
                    existingProduct.CategoryID = product.CategoryID;
                    existingProduct.Price = product.Price;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.Description = product.Description;
                    existingProduct.ImageURL = product.ImageURL;
                    existingProduct.LastUpdated = DateTime.Now;

                    // Lưu thay đổi vào cơ sở dữ liệu
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Product has been updated successfully.";
                    return RedirectToAction("EditProduct");
                }
                else
                {
                    return NotFound();
                }
            }

            return View(product); // Nếu có lỗi thì trả về view cùng với thông báo lỗi
        }

        // Hiển thị trang xác nhận xóa sản phẩm
        [HttpGet]
        public IActionResult DeleteProduct(int id)
        {
            // Lấy sản phẩm từ cơ sở dữ liệu theo ID
            var product = _context.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return NotFound(); // Nếu không tìm thấy sản phẩm, trả về 404
            }

            return View(product); // Trả về form xác nhận xóa
        }

        // Xử lý khi người dùng xác nhận xóa sản phẩm
        [HttpPost, ActionName("DeleteProduct")]
        public IActionResult DeleteProductConfirmed(int id)
        {
            // Kiểm tra quyền Admin
            if (HttpContext.Session.GetString("UserType") != "Admin")
            {
                return Unauthorized();
            }

            // Lấy sản phẩm từ cơ sở dữ liệu theo ID
            var product = _context.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return NotFound(); // Nếu không tìm thấy sản phẩm, trả về 404
            }

            // Xóa sản phẩm khỏi cơ sở dữ liệu
            _context.Products.Remove(product);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Product has been deleted successfully."; // Thông báo thành công
            return RedirectToAction("ManageProducts"); // Chuyển hướng về trang quản lý sản phẩm
        }







    }


}
