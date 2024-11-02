using System.ComponentModel.DataAnnotations;

namespace BlueSports.Models
{
    public class ShoppingCart
    {
        [Key]
        public int CartID { get; set; }
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedDate { get; set; }

        public User User { get; set; }
        public Product Product { get; set; }
    }
}
