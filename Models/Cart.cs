using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shofy.Models
{
    public class Cart
    {
        [Key]
        public int CartID { get; set; }


        [ForeignKey("User")]
        public int UserId { get; set; }

        
        public User User { get; set; }

        public ICollection<CartItem> CartItems { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}