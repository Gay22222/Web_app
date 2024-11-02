using BlueSports.Models;

namespace BlueSports.HandleAdmin.ModelViews
{
    public class CartItem
    {
        public Product? product { get; set; }
        public int amount { get; set; }
        public decimal TotalMoney => amount * product.Price;

    }

}


