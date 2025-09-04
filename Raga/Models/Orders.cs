namespace Afrodite.Models
{
    public class Orders
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
        public string Status { get; set; } = "Chờ xác nhận"; 

        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; } 

        [Required]
        [StringLength(255)]
        public string DeliveryAddress { get; set; } 

        [ForeignKey("UserId")] 
        public virtual Users User { get; set; } 

        public virtual ICollection<OrderItems> OrderItems { get; set; } = new List<OrderItems>(); 

        public void CalculateTotalAmount(decimal shippingCost, decimal couponDiscount)
        {
            TotalAmount = OrderItems.Sum(item => item.Price * item.Quantity) + shippingCost - couponDiscount;
        }
    }
}