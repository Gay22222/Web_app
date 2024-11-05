using BlueSports.HandleAdmin.Extension;
using BlueSports.HandleAdmin.ModelViews;
using Microsoft.AspNetCore.Mvc;

namespace BlueSports.Controllers.Components
{
    public class HeaderCartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            return View(cart);
        }
    }
}
