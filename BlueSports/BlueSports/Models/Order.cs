namespace BlueSports.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public int ShippingStatus { get; set; }
        public string TrackingNumber { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        public User User { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }


        public string ShippingStatusText
        {
            get
            {
                return GetShippingStatus(ShippingStatus);
            }
        }

        private string GetShippingStatus(int status)
        {
            return status switch
            {
                1 => "Đang xử lý",
                2 => "Đang vận chuyển",
                3 => "Đã giao hàng",
                _ => "Thất lạc"
            };
        }
    }
}
