using AspNetCoreHero.ToastNotification.Abstractions;
using BlueSports.Data;
using BlueSports.HandleAdmin.Extension;
using BlueSports.HandleAdmin.ModelViews;
using Microsoft.AspNetCore.Mvc;


namespace BlueSports.Controllers
{

    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly INotyfService _notyfService;
        public ShoppingCartController(ApplicationDbContext context, INotyfService notyfService)
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
        [HttpPost]
        [Route("api/cart/add")]
        public IActionResult AddToCart(int id, int? amount)
        {
            List<CartItem> cart = GioHang;

            try
            {
                // Tìm sản phẩm trong giỏ hàng
                CartItem item = cart.FirstOrDefault(p => p.product.ProductID == id);

                if (item != null) // Nếu đã có sản phẩm trong giỏ, cập nhật số lượng
                {
                    item.amount += amount ?? 1;
                    HttpContext.Session.Set<List<CartItem>>("GioHang", cart);
                }
                else
                {
                    // Tìm sản phẩm trong database
                    var hh = _context.Products
                    .FirstOrDefault(x => x.ProductID == id);

                    // Kiểm tra nếu sản phẩm tồn tại
                    if (hh == null)
                    {
                        return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                    }

                    // Tạo mới CartItem và thêm vào giỏ hàng
                    item = new CartItem
                    {
                        product = hh,
                        amount = amount ?? 1
                    };
                    cart.Add(item);
                }

                // Lưu lại Session
                HttpContext.Session.Set<List<CartItem>>("GioHang", cart);
                _notyfService.Success("Thêm sản phẩm thành công");
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }


        [HttpPost]
        [Route("api/cart/remove")]
        public ActionResult Remove(int id)
        {

            try
            {
                List<CartItem> gioHang = GioHang;
                CartItem item = gioHang.SingleOrDefault(p => p.product.ProductID == id);
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

        [HttpPost]
        [Route("api/cart/update")]
        public IActionResult UpdateCart(int id, int amount)
        {
            try
            {
                // Lấy giỏ hàng từ Session
                List<CartItem> gioHang = HttpContext.Session.Get<List<CartItem>>("GioHang") ?? new List<CartItem>();

                // Tìm sản phẩm trong giỏ hàng dựa trên ProductId (id)
                CartItem item = gioHang.SingleOrDefault(p => p.product.ProductID == id);

                if (item != null)
                {
                    // Cập nhật số lượng sản phẩm
                    item.amount = amount;


                    // Lưu lại giỏ hàng vào Session
                    HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);

                    // Trả về JSON thông báo thành công
                    return Json(new { success = true });
                }
                else
                {
                    // Nếu không tìm thấy sản phẩm, trả về lỗi
                    return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng" });
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về JSON thông báo thất bại
                return Json(new { success = false, message = "Cập nhật giỏ hàng thất bại", error = ex.Message });
            }
        }

        [Route("cart.html", Name = "Cart")]
        public IActionResult Index()
        {
            return View(GioHang);
        }
    }
}
