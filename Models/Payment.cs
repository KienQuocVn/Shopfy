using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shofy.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        [ForeignKey("Order")]
        public int OrderID { get; set; }

        [Required, Range(0.01, double.MaxValue), Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public required string Method { get; set; }

        [Required]
        public required string Status { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Order? Order { get; set; }
    }
}