using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shofy.Models
{
    public class Review
    {
        [Key]
        public int ReviewID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [ForeignKey("Product")]
        public int ProductID { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }


        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public User? User { get; set; }
        public Product? Product { get; set; }
    }
}