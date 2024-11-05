using AspNetCoreHero.ToastNotification.Abstractions;
using BlueSports.Data;
using BlueSports.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BlueSports.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        public INotyfService _notyfService { get; }
        public SearchController(ApplicationDbContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }
        
        [HttpGet]
        public async Task<IActionResult> SearchProduct(string keyword)
        {
            IQueryable<Product> query = _context.Products.Include(p => p.Category);

            if (!string.IsNullOrEmpty(keyword))
            {
                // Nếu có keyword, thực hiện tìm kiếm sản phẩm theo từ khóa
                query = query.Where(p => p.ProductName.Contains(keyword) || p.Description.Contains(keyword));
                ViewBag.Keyword = keyword; // Truyền keyword vào ViewBag để hiển thị
            }
            else
            {
                ViewBag.Keyword = "All Products"; // Hiển thị "All Products" nếu không có từ khóa
            }
            var categories = _context.Categories.ToList();
            ViewBag.Categories = categories;

            var products = await query
                .OrderByDescending(p => p.ProductName)
                .Take(18)
                .ToListAsync();

            return View(products);
        }

    }
}