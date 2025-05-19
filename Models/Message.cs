using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shofy.Models
{
    public class Message
    {
        [Key]
        public int MessageID { get; set; }

        [ForeignKey("Sender")]
        public int SenderID { get; set; }

        [ForeignKey("Receiver")]
        public int ReceiverID { get; set; }

        [Required]
        public required string Content { get; set; }

        public DateTime SentDate { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        [ForeignKey("Order")]
        public int? OrderID { get; set; } 

        public User? Sender { get; set; }
        public User? Receiver { get; set; }
        public Order? Order { get; set; }

        public string? GroupName { get; set; }
    }
}