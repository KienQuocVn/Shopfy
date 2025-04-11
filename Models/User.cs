using System;
using System.ComponentModel.DataAnnotations;

namespace Shofy.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(50)]
        public required string Username { get; set; }
        [Required]
        public required string PasswordHash { get; set; }
        [Required, EmailAddress]
        public required string Email { get; set; }
        [Required]
        public required string Role { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        [Phone]
        public string PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Cart> Carts { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}