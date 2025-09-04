namespace Afrodite.DTOs
{
    public class CreateProductDto
        {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
        public int ProductId { get; set; }

        public string Name { get; set; }


        public decimal Price { get; set; }


        public string ImageUrl { get; set; }

        public string ShortDescription { get; set; }


        public string Description { get; set; }


        public int Stock { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }


        public int BrandId { get; set; }


        public string BrandName { get; set; }


        public List <CategoryDto> Categories { get; set; }


        
        public List <BrandDto> Brand { get; set; }
    }
}