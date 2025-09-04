using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Afrodite.DTOs.AdminDtos
{
    public class OrderDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int OrderId { get; set; }

        [Required]
        public string UserId { get; set; } 

        public DateTime OrderDate { get; set; } = DateTime.Now; 

        [Column(TypeName = "decimal(18,2)")] 
        public decimal TotalAmount { get; set; } 

        [Required]
        [StringLength(50)]
        public string Status { get; set; } 

        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; } 
        [Required]
        [StringLength(255)]
        public string DeliveryAddress { get; set; } 

         public virtual ICollection<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();

        public void CalculateTotalAmount(decimal shippingCost, decimal couponDiscount)
        {
            TotalAmount = OrderItems.Sum(item => item.Price * item.Quantity) + shippingCost - couponDiscount;
        }
    }

    public class OrderItemDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Tự động tạo OrderItemId
        public int OrderItemId { get; set; }

        [Required]
        public int ProductId { get; set; } // Khóa ngoại đến Products

        [Required]
        public int Quantity { get; set; } // Số lượng sản phẩm

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Định dạng số thập phân
        public decimal Price { get; set; } // Giá sản phẩm tại thời điểm đặt hàng

        // Thêm thông tin sản phẩm
        public string ProductName { get; set; } // Tên sản phẩm
        public string ProductImageUrl { get; set; } // URL hình ảnh sản phẩm
    }
}