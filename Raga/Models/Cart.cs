namespace Afrodite.Models
{
    public class Cart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int CartId { get; set; }

        [Required]
        public string UserId { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now; 

        [ForeignKey("UserId")] 
        public virtual Users User { get; set; } 
    }
}