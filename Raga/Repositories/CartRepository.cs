using Afrodite.Models;
using Afrodite.Extensions;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Afrodite.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartRepository(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public List<OrderItems> GetCart()
        {
            return _httpContextAccessor.HttpContext.Session.GetObjectFromJson<List<OrderItems>>("Cart") ?? new List<OrderItems>();
        }

        public void SaveCart(List<OrderItems> cart)
        {
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson("Cart", cart);
        }
    }
}
