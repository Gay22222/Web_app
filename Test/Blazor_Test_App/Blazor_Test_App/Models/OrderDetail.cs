﻿namespace Blazor_Test_App.Models
{
    public class OrderDetail
    {
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }

        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}