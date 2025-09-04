using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Afrodite.Models
{
    public class Products
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] 
        public decimal Price { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        [StringLength(200)]
        public string ShortDescription { get; set; }
        [Required]
        public string Description { get; set; }

        public int Stock { get; set; } 

        [Required]
        public int CategoryId { get; set; } 

        [ForeignKey("CategoryId")] 
        public virtual Categories Category { get; set; } 

        [Required]
        public int BrandId { get; set; } 

        [ForeignKey("BrandId")] 
        public virtual Brand Brand { get; set; } 

        public virtual ICollection<OrderItems> OrderItems { get; set; } = new List<OrderItems>(); 
    }
}