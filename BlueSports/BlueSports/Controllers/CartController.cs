using Microsoft.AspNetCore.Mvc;
using BlueSports.Models;
using System.Collections.Generic;
using BlueSports.Data;
using BlueSports.HandleAdmin.Extension;
using BlueSports.HandleAdmin.ModelViews;
using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Notyf;

namespace BlueSports.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public INotyfService _notyfService { get; }

        public CartController(ApplicationDbContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        public List<CartItem> GioHang
        {
            get
            {
                var gh = HttpContext.Session.Get<List<CartItem>>("GioHang");
                if (gh == default(List<CartItem>))
                {
                    gh = new List<CartItem>();
                }
                return gh;
            }
        }

        // Action để thêm sản phẩm vào giỏ hàng
        [HttpPost]
        [Route("api/cart/add")]
        public IActionResult AddToCart(int productID, int? amount)
        {
            //List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>("GioHang") ?? new List<CartItem>();
            List<CartItem> cart = GioHang;


            try
            {
                CartItem item = cart.SingleOrDefault(p => p.product.ProductID == productID);

                if (item != null)
                {
                    // Cập nhật số lượng sản phẩm nếu đã tồn tại trong giỏ
                    item.amount += amount ?? 1;
                }
                else
                {
                    // Lấy sản phẩm từ database
                    Product hh = _context.Products.SingleOrDefault(p => p.ProductID == productID);

                    if (hh != null)
                    {
                        // Tạo mới CartItem và thêm vào giỏ
                        item = new CartItem
                        {
                            amount = amount.HasValue? amount.Value : 1,
                            product = hh
                        };
                        cart.Add(item);
                        
                    }
                    else
                    {
                        return Json(new { success = false, message = "Product not found" });
                    }
                }

                // Lưu lại giỏ hàng trong Session
                _notyfService.Success("Thêm sản phẩm thành công");
                HttpContext.Session.Set("GioHang", cart);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/cart/remove")]
        public ActionResult Remove(int productID)
        {

            try
            {
                List<CartItem> gioHang = GioHang;
                CartItem item = gioHang.SingleOrDefault(p => p.product.ProductID == productID);
                if (item != null)
                {
                    gioHang.Remove(item);
                }
                //luu lai session
                HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
        [Route("cart.html", Name = "Cart")]
        public IActionResult Index()
        {
            return View(GioHang);
        }
    }
}
