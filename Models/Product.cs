using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shofy.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required, StringLength(100)]
        public required string Name { get; set; }

        [Required, StringLength(1000)]
        public required string Description { get; set; }

        [Required, Range(0.01, double.MaxValue), Column(TypeName = "decimal(18, 2)")]
        public required decimal Price { get; set; }

        [Required, Range(0, int.MaxValue)]
        public required int StockQuantity { get; set; } 

        [Required]
        public required string ImagePath { get; set; }

        public required string Status { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public ICollection<CartItem>? CartItems { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }

}