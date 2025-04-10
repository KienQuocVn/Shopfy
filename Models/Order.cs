using System;
using System.ComponentModel.DataAnnotations;

namespace Shofy.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required int UserId { get; set; }
        [Required, Range(0, double.MaxValue)]
        public required decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        [Required]
        public required string Status { get; set; } // "Pending", "Shipped", etc.
        public string? ShippingAddress { get; set; }
        public string PaymentMethod { get; set; } // "COD", "CreditCard", etc.
        public virtual User? User { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual Payment? Payment { get; set; }
    }

}