using System;
using System.ComponentModel.DataAnnotations;

namespace ECommerceAllInOne.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100)]
        public required string Name { get; set; }
        [Required]
        public required string Description { get; set; }
        [Required, Range(0, double.MaxValue)]
        public required decimal Price { get; set; }
        [Required, Range(0, int.MaxValue)]
        public required int Stock { get; set; }
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public string Status { get; set; } 
        [Required]
        public required string ImagePath { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Cart> Carts { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }

}