namespace Blazor_Test_App.Models
{
    public class ShoppingCart
    {
        public int CartID { get; set; }
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedDate { get; set; }

        public User User { get; set; }
        public Product Product { get; set; }
    }
}
