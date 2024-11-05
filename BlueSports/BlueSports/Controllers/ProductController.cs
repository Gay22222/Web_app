using BlueSports.Data;
using BlueSports.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;

namespace BlueSports.Controllers
{
    public class ProductController : Controller
    {
        // Danh sách sản phẩm mẫu
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;

        }


        [Route("ProductList", Name = "ShopProduct")]
        public IActionResult Index(int? page)
        {
            try
            {
                // Thiết lập phân trang
                int pageNumber = page ?? 1;
                int pageSize = 10;
                ViewBag.Categories = _context.Categories;
                // Lấy danh sách sản phẩm và sắp xếp theo CreatedDate
                var products = _context.Products
                    .Include(p => p.Category)
                    .AsNoTracking()
                    .OrderBy(x => x.DateAdded);

                // Áp dụng phân trang
                IPagedList<Product> models = new PagedList<Product>(products, pageNumber, pageSize);

                // Thiết lập dữ liệu danh mục cho ViewBag
                ViewBag.CurrentPage = pageNumber;
                ViewData["DanhMuc"] = new SelectList(_context.Categories, "CategoryID", "CategoryName");

                return View(models);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                Console.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("Index", "Home");
            }
        }




        [Route("/{productName}-{id:int}", Name = "ProductDetails")]
        public IActionResult Details(int id)
        {
            try
            {
                // Lấy sản phẩm và bao gồm thông tin danh mục (Cate)
                var product = _context.Products
                    .Include(x => x.Category) // Điều chỉnh nếu tên bảng là "Category"
                    .FirstOrDefault(x => x.ProductID == id);
                
                
                // Kiểm tra nếu sản phẩm không tồn tại
                if (product == null)
                {
                    return RedirectToAction("Index");
                }

                // Tạo SelectList cho danh mục nếu cần hiển thị danh mục khác
                ViewData["DanhMuc"] = new SelectList(_context.Categories, "CategoryID", "CategoryName");

                // Trả về View với model product
                return View(product);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                Console.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("Index", "Home");
            }
        }


        

        public IActionResult Filter(int categoryId = 0)
        {
            string url;

            if (categoryId > 0)
            {
                url = $"/danhmuc/{categoryId}";
            }
            else
            {
                url = "/ProductList";
            }

            return Json(new { status = "success", redirecturl = url });
        }

        [Route("danhmuc/{categoryId}", Name = "ListProduct")]
        public IActionResult List(int categoryId, int page = 1)
        {
            try
            {
                var pageSize = 10;
                var category = _context.Categories.AsNoTracking().SingleOrDefault(x => x.CategoryID == categoryId);

                if (category == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                var lsTinDangs = _context.Products
                    .Include(x => x.Category)
                    .AsNoTracking()
                    .Where(x => x.CategoryID == categoryId)
                    .OrderByDescending(x => x.DateAdded);

                PagedList<Product> model = new PagedList<Product>(lsTinDangs, page, pageSize);
                ViewBag.CurrentPage = page;
                ViewBag.Category = category;

                var categories = _context.Categories.ToList();
                ViewBag.Categories = categories;


                ViewData["DanhMuc"] = new SelectList(_context.Categories, "CategoryID", "Name");

                return View(model);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpGet]
        [Route("Product/FilterAndSort", Name = "FilterAndSort")]
        public IActionResult FilterAndSort(int? categoryId = null, int? brandId = null, int sortOption = 0, int page = 1)
        {
            try
            {
                const int pageSize = 10;

                IQueryable<Product> products = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .AsNoTracking();

                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    products = products.Where(p => p.CategoryID == categoryId);
                }

                if (brandId.HasValue && brandId.Value > 0)
                {
                    products = products.Where(p => p.BrandID == brandId);
                }

                products = sortOption switch
                {
                    2 => products.OrderBy(p => p.Price),
                    3 => products.OrderByDescending(p => p.Price),
                    _ => products.OrderByDescending(p => p.DateAdded)
                };

                IPagedList<Product> model = new PagedList<Product>(products, page, pageSize);

                var resultData = model.Select(p => new
                {
                    p.ProductID,
                    p.ProductName,
                    p.Price,
                    p.ImageURL,
                    CategoryName = p.Category.CategoryName,
                    BrandName = p.Brand.BrandName
                }).ToList();

                return PartialView("_ProductListPartial", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { status = "error", message = "Error in filtering and sorting" });
            }
        }










        // Phần dưới đây dùng cho admin



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
        //public IActionResult Details(int id)
        //{
        //    var product = _context.Products.FirstOrDefault(p => p.ProductID == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(product);
        //}

        public IActionResult ManageProducts()
        {
            var products = _context.Products.ToList();
            return View(products); // Hiển thị danh sách sản phẩm hoặc các chức năng quản lý sản phẩm
        }


        // GET: Hiển thị form chỉnh sửa sản phẩm theo ID
        public IActionResult EditProduct(int id)
        {
            // Lấy sản phẩm theo ID
            var existingProduct = _context.Products.FirstOrDefault(p => p.ProductID == id);
            if (existingProduct == null)
            {
                return NotFound(); // Nếu không tìm thấy sản phẩm, trả về 404
            }
            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.Categories = _context.Categories.ToList();

            return View(existingProduct); // Trả về view cùng dữ liệu của sản phẩm
        }

        // POST: Xử lý cập nhật sản phẩm
        [HttpPost]
        public IActionResult EditProduct(int id, string productName, decimal price, int stockQuantity, string description, string imageUrl, int brandId, int categoryId)
        {
            // Kiểm tra quyền Admin
            if (HttpContext.Session.GetString("UserType") != "Admin")
            {
                return Unauthorized(); // Trả về lỗi nếu không phải Admin
            }

            // Lấy sản phẩm từ cơ sở dữ liệu theo ID
            var existingProduct = _context.Products.FirstOrDefault(p => p.ProductID == id);
            if (existingProduct == null)
            {
                return NotFound(); // Nếu không tìm thấy sản phẩm, trả về 404
            }

            // Cập nhật thông tin sản phẩm
            existingProduct.ProductName = productName;
            existingProduct.Price = price;
            existingProduct.StockQuantity = stockQuantity;
            existingProduct.Description = description;
            existingProduct.ImageURL = imageUrl;
            existingProduct.BrandID = brandId;
            existingProduct.CategoryID = categoryId;

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.Products.Update(existingProduct);
            _context.SaveChanges();

            // Thông báo thành công
            TempData["SuccessMessage"] = "Product has been updated successfully.";

            return RedirectToAction("ManageProducts"); // Chuyển hướng về trang quản lý sản phẩm
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
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Product has been deleted successfully."; // Thông báo thành công
            }

            return RedirectToAction("ManageProducts"); // Chuyển hướng về trang quản lý sản phẩm
        }







    }


}
