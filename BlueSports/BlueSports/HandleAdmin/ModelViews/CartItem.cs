using BlueSports.Models;

namespace BlueSports.HandleAdmin.ModelViews
{
    public class CartItem
    {
        public Product product { get; set; }
        public int amount { get; set; }
        public decimal TotalMoney
        {
            get
            {
                return amount * product.Price;
            }
        }
    }
}
