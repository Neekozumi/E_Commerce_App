using Afrodite.Models;
using System.Threading.Tasks;

namespace Afrodite.Services
{
    public interface ICartService
    {
        Task AddToCartAsync(int productId);
    }
}
