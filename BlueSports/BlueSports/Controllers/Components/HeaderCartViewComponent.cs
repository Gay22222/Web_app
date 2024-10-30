using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BlueSports.HandleAdmin.Extension;
using BlueSports.HandleAdmin.ModelViews;

namespace RoboTech.Controllers.Components
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
