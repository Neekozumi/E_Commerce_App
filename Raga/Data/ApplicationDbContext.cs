namespace Afrodite.Data
{
    public class ApplicationDbContext : IdentityDbContext<Users , IdentityRole, string> 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TransactionHistories> TransactionHistories { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderItems> OrderItems { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Brand> Brand { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Coupon> Coupons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Orders>()
                .HasOne(o => o.User) 
                .WithMany(u => u.Orders) 
                .HasForeignKey(o => o.UserId);

            modelBuilder.Entity<OrderItems>()
                .HasOne(oi => oi.Order) 
                .WithMany(o => o.OrderItems) 
                .HasForeignKey(oi => oi.OrderId); 

            modelBuilder.Entity<OrderItems>()
                .HasOne(oi => oi.Product) 
                .WithMany(p => p.OrderItems) 
                .HasForeignKey(oi => oi.ProductId); 

            modelBuilder.Entity<Products>()
                .HasOne(p => p.Category) 
                .WithMany(c => c.Products) 
                .HasForeignKey(p => p.CategoryId); 

            modelBuilder.Entity<TransactionHistories>()
                .HasOne(th => th.User) 
                .WithMany(u => u.TransactionHistories) 
                .HasForeignKey(th => th.UserId); 

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User) 
                .WithMany(u => u.Carts) 
                .HasForeignKey(c => c.UserId); 


            modelBuilder.Entity<IdentityUserRole<string>>()
                   .HasOne<IdentityRole>()
                   .WithMany()
                   .HasForeignKey(ur => ur.RoleId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}