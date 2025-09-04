using Afrodite.Models;
using Afrodite.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace Afrodite.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ApplicationDbContext _context;

        public CartService(ICartRepository cartRepository, ApplicationDbContext context)
        {
            _cartRepository = cartRepository;
            _context = context;
        }

        public async Task AddToCartAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            var cart = _cartRepository.GetCart();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (cartItem == null)
            {
                cart.Add(new OrderItems
                {
                    ProductId = product.ProductId,
                    Quantity = 1,
                    Price = product.Price,
                    Product = product
                });
            }
            else
            {
                cartItem.Quantity++;
            }

            _cartRepository.SaveCart(cart);
        }
    }
}
