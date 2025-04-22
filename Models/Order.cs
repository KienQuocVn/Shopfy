using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shofy.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [Required, Range(0.01, double.MaxValue), Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public required string Status { get; set; }

        [Required]
        public required string PaymentMethod { get; set; }
        
        public DateTime OrderedDate { get; set; } = DateTime.Now;
        public User? User { get; set; }
        public Payment? Payment { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }

}