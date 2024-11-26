using AspNetCoreHero.ToastNotification.Abstractions;
using BlueSports.Data;
using BlueSports.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace BlueSports.Controllers
{
    public class ProductController : Controller
    {
        // Danh sách sản phẩm mẫu
        private readonly ApplicationDbContext _context;
        private readonly INotyfService _notyfService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(ApplicationDbContext context, INotyfService notyfService, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _notyfService = notyfService;
            _webHostEnvironment = webHostEnvironment;
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


        // Tạo thư mục tạm 

        private string CreateTempFolder()
        {
            var tempFolder = Path.Combine(_webHostEnvironment.WebRootPath, "temp");
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder); // Tạo thư mục nếu chưa tồn tại
            }
            return tempFolder;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                _notyfService.Warning("Vui lòng chọn một file hợp lệ!");
                return RedirectToAction("Index"); // Chuyển hướng về trang chính
            }

            // Lấy đường dẫn thư mục tạm
            var tempFolder = CreateTempFolder();

            // Tạo tên file duy nhất
            var uniqueFileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
            var filePath = Path.Combine(tempFolder, uniqueFileName);

            // Lưu file lên thư mục tạm
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            _notyfService.Success("File đã được tải lên thư mục tạm!");
            TempData["TempFilePath"] = filePath; // Lưu lại đường dẫn tạm để xử lý sau
            return RedirectToAction("Index");
        }


        // Xóa thư mục tạm
        private void DeleteTempFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        // Xử lí hình ảnh


        [HttpPost]
        public IActionResult ProcessTempImage()
        {
            var tempFilePath = TempData["TempFilePath"]?.ToString();
            if (string.IsNullOrEmpty(tempFilePath) || !System.IO.File.Exists(tempFilePath))
            {
                TempData["Error"] = "Không tìm thấy file tạm để xử lý.";
                return RedirectToAction("Index");
            }

            try
            {
                // Ví dụ: Xử lý ảnh (logic thực tế của bạn)
                var result = ProcessImage(tempFilePath);

                // Xóa file tạm sau khi xử lý xong
                DeleteTempFile(tempFilePath);

                TempData["Success"] = "Xử lý ảnh thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi trong quá trình xử lý ảnh: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // Ví dụ: Phương thức xử lý ảnh
        private string ProcessImage(string filePath)
        {
            // Logic xử lý ảnh, ví dụ phân tích bằng OpenCV hoặc AI
            // Ở đây chỉ trả về đường dẫn làm ví dụ
            return $"Đã xử lý file: {filePath}";
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                _notyfService.Warning("Vui lòng chọn một file hợp lệ!");
                return RedirectToAction("Index");
            }

            using (var client = new HttpClient())
            {
                try
                {
                    // Gửi ảnh tới REST API
                    var requestContent = new MultipartFormDataContent();
                    var imageContent = new StreamContent(imageFile.OpenReadStream());
                    requestContent.Add(imageContent, "file", imageFile.FileName);

                    var response = await client.PostAsync("http://localhost:8000/detect/", requestContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();

                        // Deserialize JSON trả về
                        var detectionResult = JsonConvert.DeserializeObject<DetectionResult>(responseData);

                        // Truyền kết quả qua View
                        return View("Result", detectionResult);
                    }
                    else
                    {
                        TempData["Error"] = "Không thể phân tích ảnh.";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Lỗi khi kết nối API: {ex.Message}";
                    return RedirectToAction("Index");
                }
            }
        }





        public IActionResult Result()
        {
            return View();
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
