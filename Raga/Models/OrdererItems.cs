namespace Afrodite.Models
{
    public class OrderItems
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int OrderItemId { get; set; }

        [Required]
        public int OrderId { get; set; } 

        [Required]
        public int ProductId { get; set; } 

        [Required]
        public int Quantity { get; set; } 

        [Required]
        [Column(TypeName = "decimal(18,2)")] 
        public decimal Price { get; set; } 

        [ForeignKey("OrderId")] 
        public virtual Orders Order { get; set; } 

        [ForeignKey("ProductId")] 
        public virtual Products Product { get; set; } 
    }
}