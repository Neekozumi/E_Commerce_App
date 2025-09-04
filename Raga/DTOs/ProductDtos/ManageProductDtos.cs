 namespace Afrodite.DTOs
{
    public class ManageProductDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tên sản phẩm phải từ 3-200 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")]
        public decimal Price { get; set; }


        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Mô tả ngắn là bắt buộc")]
        [StringLength(500, ErrorMessage = "Mô tả ngắn không được quá 500 ký tự")]
        public string ShortDescription { get; set; }

        [Required(ErrorMessage = "Mô tả chi tiết là bắt buộc")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho phải lớn hơn hoặc bằng 0")]
        public int Stock { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }


        public int BrandId { get; set; }


        public string BrandName { get; set; }


        public CategoryDto Categories { get; set; }


        
        public BrandDto Brand { get; set; }
    }
}