namespace Afrodite.Models
{
    public class Users : IdentityUser 
    {


        public string? FullName { get; set; }

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now; 

        public DateTime HireDate { get; set; } = DateTime.Now; 

        public virtual ICollection<Orders> Orders { get; set; } = new List<Orders>(); 

        public virtual ICollection<TransactionHistories> TransactionHistories { get; set; } = new List<TransactionHistories>(); 

        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>(); 
    }
}