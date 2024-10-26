namespace BlueSports.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }

        // Điều hướng liên kết với các sản phẩm thuộc danh mục này
        public ICollection<Product> Products { get; set; }
    }
}
