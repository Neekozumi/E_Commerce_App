using Microsoft.AspNetCore.Mvc;
using Afrodite.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Afrodite.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OrderDetails(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderId == orderId)
                .Select(o => new Afrodite.DTOs.AdminDtos.OrderDto
                {
                    OrderId = o.OrderId,
                    UserId = o.UserId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    PhoneNumber = o.PhoneNumber,
                    DeliveryAddress = o.DeliveryAddress,
                    OrderItems = o.OrderItems.Select(oi => new Afrodite.DTOs.AdminDtos.OrderItemDto
                    {
                        ProductName = oi.Product.Name,
                        ProductImageUrl = oi.Product.ImageUrl,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList()
                })
                .FirstOrDefault();

            if (order == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("Index");
            }

            return View(new List<Afrodite.DTOs.AdminDtos.OrderDto> { order }); 
        }
    }
}