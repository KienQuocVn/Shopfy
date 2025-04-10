using System;
using System.ComponentModel.DataAnnotations;

namespace Shofy.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public virtual Category? ParentCategory { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
