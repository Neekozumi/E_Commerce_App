namespace Afrodite.DTOs
{
    public class BrandDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
        public int BrandId { get; set; }

        public string Name { get; set; }
         
        public virtual ICollection<Products> Products { get; set; } = new List<Products>();  
    }
}