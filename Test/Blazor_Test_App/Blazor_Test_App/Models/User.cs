using System.ComponentModel.DataAnnotations;

namespace Blazor_Test_App.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string PhoneNumber { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime DateJoined { get; set; } = DateTime.Now;

        // Phân quyền người dùng (Admin/Customer)
        public string UserType { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}
