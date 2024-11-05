using BlueSports.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlueSports.HandleAdmin.ModelViews
{
    public class XemDonHang 
    {
        public Order DonHang { get; set; }
        public List<OrderDetail> ChiTietDonHang { get; set; }
    }
}
