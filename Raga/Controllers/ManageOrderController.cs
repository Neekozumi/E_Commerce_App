using Microsoft.AspNetCore.Mvc; 
using Afrodite.Models; 
using Afrodite.DTOs.AdminDtos; 
using System.Linq; 
using System.Collections.Generic; 
using System.Security.Claims;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Afrodite.Controllers
{
    public class ManageOrderController : Controller
    {
        private readonly ApplicationDbContext _context; 

        public ManageOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Order()
        {
            var orders = _context.Orders
                .Select(o => new OrderDto
                {
                    OrderId = o.OrderId,
                    UserId = o.UserId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status ?? "Chờ xác nhận", 
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        OrderItemId = oi.OrderItemId,
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList()
                }).ToList();

            ViewData["OrderStatuses"] = new List<string> { "Chờ xác nhận", "Đang giao", "Đã giao", "Đã hủy" };

            return View(orders);
        }

        public IActionResult Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Home");
            }

            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Select(o => new OrderDto
                {
                    OrderId = o.OrderId,
                    UserId = o.UserId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    PhoneNumber = o.PhoneNumber, 
                    DeliveryAddress = o.DeliveryAddress, 
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        Price = oi.Price,
                        ProductName = oi.Product.Name,
                        ProductImageUrl = oi.Product.ImageUrl
                    }).ToList()
                })
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("UserOrders");
            }

            decimal shippingCost = 0; 
            decimal couponDiscount = 0; 
            order.CalculateTotalAmount(shippingCost, couponDiscount);

            var orderDto = new OrderDto
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderItems = order.OrderItems.Select(item => new OrderItemDto
                {
                    ProductName = item.ProductName, 
                    ProductImageUrl = item.ProductImageUrl, 
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList()
            };

            return View(order); 
        }

        [HttpPost]
        public IActionResult UpdateStatus(int id, string status)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == id);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Order");
            }
            
            order.Status = status; 
            _context.SaveChanges();

            
            TempData["Success"] = $"Trạng thái đơn hàng đã được cập nhật thành '{status}'.";
            return RedirectToAction("Order");
        }

        [HttpPost]
        public IActionResult DeleteOrder(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            _context.SaveChanges();

            return RedirectToAction("Order");
        }

        public IActionResult UserOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Home");
            }

            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .Select(o => new OrderDto
                {
                    OrderId = o.OrderId,
                    UserId = o.UserId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        OrderItemId = oi.OrderItemId,
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList()
                }).ToList();

            return View(orders);
        }

        [HttpPost]
        public IActionResult CancelOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Bạn cần đăng nhập để hủy đơn hàng.";
                return RedirectToAction("UserOrders");
            }

            var order = _context.Orders.FirstOrDefault(o => o.OrderId == id && o.UserId == userId);
            if (order == null || order.Status == "Completed")
            {
                TempData["Error"] = "Không thể hủy đơn hàng này.";
                return RedirectToAction("UserOrders");
            }

            order.Status = "Đã hủy";
            _context.SaveChanges();

            TempData["Success"] = "Đơn hàng đã được hủy.";
            return RedirectToAction("UserOrders");
        }

        public async Task<IActionResult> CalculateShippingCost(string customerAddress)
        {
            string fixedAddress = "189 Cống Quỳnh, Phường Nguyễn Cư Trinh, Quận 1, Hồ Chí Minh 700000, Việt Nam";

            var fixedCoordinates = await GetCoordinatesFromAddress(fixedAddress);
            var customerCoordinates = await GetCoordinatesFromAddress(customerAddress);

            if (fixedCoordinates == null || customerCoordinates == null)
            {
                return Json(new { success = false, message = "Không thể xác định tọa độ của địa chỉ." });
            }

            double distance = CalculateDistance(fixedCoordinates.Value.lat, fixedCoordinates.Value.lng, customerCoordinates.Value.lat, customerCoordinates.Value.lng);

            decimal shippingCost = 0;
            if (distance <= 5)
            {
                shippingCost = 20000 + (decimal)(distance * 3000); 
            }
            else if (distance <= 10)
            {
                shippingCost = 30000 + (decimal)((distance - 5) * 4000); 
            }
            else if (distance <= 20)
            {
                shippingCost = 40000 + (decimal)((distance - 10) * 3000); 
            }
            else if (distance <= 50)
            {
                shippingCost = 50000 + (decimal)((distance - 20) * 2000); 
            }
            else if (distance <= 100)
            {
                shippingCost = 100000 + (decimal)((distance - 50) * 1600); 
            }
            else if (distance <= 300)
            {
                shippingCost = 150000 + (decimal)((distance - 100) * 750); 
            }
            else
            {
                shippingCost = 250000 + (decimal)((distance - 300) * 833);
            }

            return Json(new { success = true, shippingCost = shippingCost.ToString("N0") + " VND" });
        }

        [HttpPost]
        [ActionName("ProcessCheckout")]
        public async Task<IActionResult> Checkout(string deliveryAddress)
        {
            if (string.IsNullOrEmpty(deliveryAddress))
            {
                TempData["Error"] = "Vui lòng nhập địa chỉ giao hàng.";
                return RedirectToAction("Cart", "Cart");
            }

            var shippingCostResult = await CalculateShippingCost(deliveryAddress);
            if (shippingCostResult is JsonResult jsonResult)
            {
                var result = jsonResult.Value as JObject; 
                if (result != null && result["success"]?.Value<bool>() == false)
                {
                    TempData["Error"] = result["message"]?.ToString();
                    return RedirectToAction("Cart", "Cart");
                }

                TempData["Success"] = "Đặt hàng thành công. Phí vận chuyển: " + result?["shippingCost"]?.ToString();
                return RedirectToAction("Order");
            }

            TempData["Error"] = "Đã xảy ra lỗi khi tính phí vận chuyển.";
            return RedirectToAction("Cart", "Cart");
        }

        
        private async Task<(double lat, double lng)?> GetCoordinatesFromAddress(string address)
        {
            string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&addressdetails=1";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Raga/1.0 (phat33789@gmail.com)");

            try
            {
                var response = await httpClient.GetStringAsync(url);
                var json = JArray.Parse(response);

                if (json.Count > 0)
                {
                    var location = json[0];
                    return (location["lat"]?.Value<double>() ?? 0, location["lon"]?.Value<double>() ?? 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Nominatim API: {ex.Message}");
            }

            return null; 
        }
        
        private double CalculateDistance(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371;
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLng = (lng2 - lng1) * Math.PI / 180;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}