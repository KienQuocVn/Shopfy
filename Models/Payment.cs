using System;
using System.ComponentModel.DataAnnotations;

namespace Shofy.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}