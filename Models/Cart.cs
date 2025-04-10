using System;
using System.ComponentModel.DataAnnotations;

namespace Shofy.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; } // Khóa ngoại tới User
        public virtual User User { get; set; }

        public int ProductId { get; set; } // Khóa ngoại tới Product
        public virtual Product Product { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}