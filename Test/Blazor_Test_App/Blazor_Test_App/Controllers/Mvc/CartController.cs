using Microsoft.AspNetCore.Mvc;
using Blazor_Test_App.Models;
using System.Collections.Generic;
using Blazor_Test_App.Data;

namespace Blazor_Test_App.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private static List<Product> cart = new List<Product>();

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }
        // Action để thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductID == productId);  // Lấy sản phẩm từ database
            if (product != null)
            {
                cart.Add(product);
            }

            return RedirectToAction("Index", "Cart");
        }

        // Hiển thị giỏ hàng
        public IActionResult Index()
        {
            return View(cart);
        }
    }
}
