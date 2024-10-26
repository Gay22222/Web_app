namespace BlueSports.Models
{
    public class Brand
    {
        public int BrandID { get; set; }
        public string BrandName { get; set; }
        public string Description { get; set; }

        // Điều hướng liên kết với các sản phẩm thuộc thương hiệu này
        public ICollection<Product> Products { get; set; }
    }
}
