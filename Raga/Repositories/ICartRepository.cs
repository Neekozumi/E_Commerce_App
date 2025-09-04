using Afrodite.Models;
using System.Collections.Generic;

namespace Afrodite.Repositories
{
    public interface ICartRepository
    {
        List<OrderItems> GetCart();
        void SaveCart(List<OrderItems> cart);
    }
}
