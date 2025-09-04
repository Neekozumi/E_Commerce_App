using Microsoft.AspNetCore.Mvc;
using Afrodite.Models;
using Afrodite.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Claims;
using Stripe;
using Afrodite.Services;
using Afrodite.Commands;
using MediatR;

namespace Afrodite.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;
        private readonly IMediator _mediator;

        public CartController(ApplicationDbContext context, ICartService cartService, IMediator mediator)
        {
            _context = context;
            _cartService = cartService;
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItems>>("Cart") ?? new List<OrderItems>();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int id)
        {
            try
            {
                await _cartService.AddToCartAsync(id);
                return RedirectToAction("Index", "Home");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItems>>("Cart") ?? new List<OrderItems>();

            var cartItem = cart.FirstOrDefault(c => c.ProductId == id);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            var total = cart.Sum(c => c.Price * c.Quantity).ToString("N0");
            return Json(new { success = true, total = total, itemTotal = (cartItem.Price * cartItem.Quantity).ToString("N0") });
        }

        public class OrderFactory
        {
            public static Orders CreateOrder(string userId, string deliveryAddress, string phoneNumber, decimal shippingCost, decimal discountAmount, List<OrderItems> cart)
            {
                var order = new Orders
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    Status = "Chờ xác nhận",
                    DeliveryAddress = deliveryAddress,
                    PhoneNumber = phoneNumber,
                    OrderItems = cart.Select(c => new OrderItems
                    {
                        ProductId = c.ProductId,
                        Quantity = c.Quantity,
                        Price = c.Price
                    }).ToList()
                };

                order.CalculateTotalAmount(shippingCost, discountAmount);
                return order;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(string deliveryAddress, string couponCode, string phoneNumber)
        {
            if (string.IsNullOrEmpty(deliveryAddress))
            {
                TempData["Error"] = "Vui lòng nhập địa chỉ giao hàng.";
                return RedirectToAction("Cart");
            }

            if (string.IsNullOrEmpty(phoneNumber))
            {
                TempData["Error"] = "Vui lòng nhập số điện thoại.";
                return RedirectToAction("Cart");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Bạn cần đăng nhập để đặt hàng.";
                return RedirectToAction("Cart");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItems>>("Cart");
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Cart");
            }

            Console.WriteLine($"DeliveryAddress: {deliveryAddress}");

            if (string.IsNullOrEmpty(couponCode) && TempData.ContainsKey("AppliedCoupon"))
            {
                couponCode = TempData["AppliedCoupon"]?.ToString();
            }

            var shippingCostResult = await new ManageOrderController(_context).CalculateShippingCost(deliveryAddress);
            if (shippingCostResult is JsonResult jsonResult)
            {
                var result = jsonResult.Value as dynamic;
                if (result != null && result.success == false)
                {
                    TempData["Error"] = result.message;
                    return RedirectToAction("Cart");
                }

                string shippingCostString = result.shippingCost.ToString().Replace(" VND", "").Replace(",", "");
                if (!decimal.TryParse(shippingCostString, out decimal shippingCost))
                {
                    TempData["Error"] = "Không thể tính phí vận chuyển.";
                    return RedirectToAction("Cart");
                }

                decimal total = cart.Sum(c => c.Price * c.Quantity);

                decimal discountAmount = 0;
                if (!string.IsNullOrEmpty(couponCode))
                {
                    var coupon = _context.Coupons.FirstOrDefault(c => c.Code == couponCode && c.IsActive && c.ExpiryDate >= DateTime.Now);
                    if (coupon != null)
                    {
                        discountAmount = coupon.DiscountAmount;
                        TempData["AppliedCoupon"] = couponCode; 
                    }
                    else
                    {
                        TempData["Error"] = $"Mã coupon '{couponCode}' không hợp lệ hoặc đã hết hạn.";
                        return RedirectToAction("Cart");
                    }
                }

                decimal totalWithShipping = Math.Max(0, total + shippingCost - discountAmount);

                Console.WriteLine($"Giá trị lưu vào database (totalWithShipping): {totalWithShipping}");

                var order = OrderFactory.CreateOrder(userId, deliveryAddress, phoneNumber, shippingCost, discountAmount, cart);

                _context.Orders.Add(order);
                _context.SaveChanges(); 

                HttpContext.Session.Remove("Cart");

                TempData["Success"] = $"Đặt hàng thành công! Tổng cộng (bao gồm phí vận chuyển và giảm giá): {order.TotalAmount:N0} VND";
                return RedirectToAction("UserOrders", "ManageOrder");
            }

            TempData["Error"] = "Đã xảy ra lỗi khi tính phí vận chuyển.";
            return RedirectToAction("Cart");
        }

        public IActionResult Cart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItems>>("Cart") ?? new List<OrderItems>();
            return View(cart);
        }

        [HttpPost]
        [Route("Cart/PayWithMoMo")]
        public async Task<IActionResult> PayWithMoMo(string deliveryAddress, string phoneNumber, string couponCode)
        {
            if (string.IsNullOrEmpty(deliveryAddress))
            {
                TempData["Error"] = "Vui lòng nhập địa chỉ giao hàng.";
                return RedirectToAction("Cart");
            }

            if (string.IsNullOrEmpty(phoneNumber))
            {
                TempData["Error"] = "Vui lòng nhập số điện thoại.";
                return RedirectToAction("Cart");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Bạn cần đăng nhập để thanh toán.";
                return RedirectToAction("Cart");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItems>>("Cart");
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Cart");
            }

            
            var shippingCostResult = await new ManageOrderController(_context).CalculateShippingCost(deliveryAddress);
            if (shippingCostResult is JsonResult jsonResult)
            {
                var shippingResult = jsonResult.Value as dynamic;
                if (shippingResult != null && shippingResult.success == false)
                {
                    TempData["Error"] = shippingResult.message;
                    return RedirectToAction("Cart");
                }

                string shippingCostString = shippingResult.shippingCost.ToString().Replace(" VND", "").Replace(",", "");
                if (!decimal.TryParse(shippingCostString, out decimal shippingCost))
                {
                    TempData["Error"] = "Không thể tính phí vận chuyển.";
                    return RedirectToAction("Cart");
                }

                decimal total = cart.Sum(c => c.Price * c.Quantity);

                decimal discountAmount = 0;
                if (!string.IsNullOrEmpty(couponCode))
                {
                    var coupon = _context.Coupons.FirstOrDefault(c => c.Code == couponCode && c.IsActive && c.ExpiryDate >= DateTime.Now);
                    if (coupon != null)
                    {
                        discountAmount = coupon.DiscountAmount;
                        TempData["AppliedCoupon"] = couponCode; 
                    }
                    else
                    {
                        TempData["Error"] = $"Mã coupon '{couponCode}' không hợp lệ hoặc đã hết hạn.";
                        return RedirectToAction("Cart");
                    }
                }

                decimal totalWithShipping = Math.Max(0, total + shippingCost - discountAmount);

                try
                {
                    var endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
                    var partnerCode = "MOMO";
                    var accessKey = "F8BBA842ECF85";
                    var secretKey = "K951B6PE1waDMi640xX08PD3vg6EkVlz";
                    var orderId = Guid.NewGuid().ToString();
                    var requestId = Guid.NewGuid().ToString();
                    var redirectUrl = Url.Action("CheckoutSuccess", "Cart", null, Request.Scheme);
                    var ipnUrl = Url.Action("CheckoutSuccess", "Cart", null, Request.Scheme);
                    var requestType = "captureWallet";

                    var rawHash = $"accessKey={accessKey}&amount={(long)totalWithShipping}&extraData=&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo=Thanh toán đơn hàng&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";

                    Console.WriteLine($"RawHash: {rawHash}");

                    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
                    var signature = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawHash))).Replace("-", "").ToLower();

                    Console.WriteLine($"Signature: {signature}");

                    var requestBody = new
                    {
                        partnerCode,
                        accessKey,
                        requestId,
                        amount = ((long)totalWithShipping).ToString(), 
                        orderId,
                        orderInfo = "Thanh toán đơn hàng",
                        redirectUrl,
                        ipnUrl,
                        extraData = "",
                        requestType,
                        signature
                    };

                    Console.WriteLine($"RequestBody: {JsonConvert.SerializeObject(requestBody)}");

                    using var client = new HttpClient();
                    var response = await client.PostAsync(endpoint, new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));
                    var responseContent = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"MoMo API Response: {responseContent}");

                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    if (result?.payUrl != null)
                    {
                        var order = new Orders
                        {
                            UserId = userId,
                            OrderDate = DateTime.Now,
                            TotalAmount = totalWithShipping,
                            Status = "Chờ xác nhận",
                            DeliveryAddress = deliveryAddress,
                            PhoneNumber = phoneNumber,
                            OrderItems = cart.Select(c => new OrderItems
                            {
                                ProductId = c.ProductId,
                                Quantity = c.Quantity,
                                Price = c.Price
                            }).ToList()
                        };

                        _context.Orders.Add(order);
                        _context.SaveChanges();

                        HttpContext.Session.Remove("Cart");

                        return Redirect(result.payUrl.ToString());
                    }

                    TempData["Error"] = $"Không thể tạo liên kết thanh toán qua MoMo. Phản hồi từ API: {responseContent}";
                    return RedirectToAction("Cart");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Lỗi khi thanh toán qua MoMo: {ex.Message}";
                    return RedirectToAction("Cart");
                }
            }

            TempData["Error"] = "Đã xảy ra lỗi khi tính phí vận chuyển.";
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public async Task<IActionResult> PayWithBankCard(string stripeToken, string deliveryAddress, string phoneNumber, string couponCode)
        {
            if (string.IsNullOrEmpty(deliveryAddress))
            {
                TempData["Error"] = "Vui lòng nhập địa chỉ giao hàng.";
                return RedirectToAction("Cart");
            }

            if (string.IsNullOrEmpty(phoneNumber))
            {
                TempData["Error"] = "Vui lòng nhập số điện thoại.";
                return RedirectToAction("Cart");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Bạn cần đăng nhập để thanh toán.";
                return RedirectToAction("Cart");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItems>>("Cart");
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Cart");
            }

            var shippingCostResult = await new ManageOrderController(_context).CalculateShippingCost(deliveryAddress);
            if (shippingCostResult is JsonResult jsonResult)
            {
                var shippingResult = jsonResult.Value as dynamic;
                if (shippingResult != null && shippingResult.success == false)
                {
                    TempData["Error"] = shippingResult.message;
                    return RedirectToAction("Cart");
                }

                string shippingCostString = shippingResult.shippingCost.ToString().Replace(" VND", "").Replace(",", "");
                if (!decimal.TryParse(shippingCostString, out decimal shippingCost))
                {
                    TempData["Error"] = "Không thể tính phí vận chuyển.";
                    return RedirectToAction("Cart");
                }

                decimal total = cart.Sum(c => c.Price * c.Quantity);

                decimal discountAmount = 0;
                if (!string.IsNullOrEmpty(couponCode))
                {
                    var coupon = _context.Coupons.FirstOrDefault(c => c.Code == couponCode && c.IsActive && c.ExpiryDate >= DateTime.Now);
                    if (coupon != null)
                    {
                        discountAmount = coupon.DiscountAmount;
                        TempData["AppliedCoupon"] = couponCode; 
                    }
                    else
                    {
                        TempData["Error"] = $"Mã coupon '{couponCode}' không hợp lệ hoặc đã hết hạn.";
                        return RedirectToAction("Cart");
                    }
                }

                decimal totalWithShipping = Math.Max(0, total + shippingCost - discountAmount);

                try
                {

                    var options = new ChargeCreateOptions
                    {
                        Amount = (long)(totalWithShipping * 100), 
                        Currency = "vnd", 
                        Description = "Thanh toán đơn hàng qua thẻ ngân hàng",
                        Source = stripeToken, 
                    };

                    var service = new ChargeService();
                    Charge charge = service.Create(options);

                    if (charge.Status == "succeeded")
                    {
                        var order = new Orders
                        {
                            UserId = userId,
                            OrderDate = DateTime.Now,
                            TotalAmount = totalWithShipping,
                            Status = "Chờ xác nhận", 
                            DeliveryAddress = deliveryAddress, 
                            PhoneNumber = phoneNumber, 
                            OrderItems = cart.Select(c => new OrderItems
                            {
                                ProductId = c.ProductId,
                                Quantity = c.Quantity,
                                Price = c.Price
                            }).ToList()
                        };

                        _context.Orders.Add(order);
                        _context.SaveChanges();

                        HttpContext.Session.Remove("Cart");

                        TempData["Success"] = $"Thanh toán bằng thẻ ngân hàng thành công! Tổng cộng (bao gồm phí vận chuyển và giảm giá): {totalWithShipping:N0} VND";
                        return RedirectToAction("Cart");
                    }
                    else
                    {
                        TempData["Error"] = "Thanh toán không thành công. Vui lòng thử lại.";
                        return RedirectToAction("Cart");
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Lỗi khi thanh toán: {ex.Message}";
                    return RedirectToAction("Cart");
                }
            }

            TempData["Error"] = "Đã xảy ra lỗi khi tính phí vận chuyển.";
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult CreateCheckoutSession()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Bạn cần đăng nhập để thanh toán.";
                return RedirectToAction("Cart");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItems>>("Cart");
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Cart");
            }

            var domain = $"{Request.Scheme}://{Request.Host}";

         

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = cart.Select(item => new Stripe.Checkout.SessionLineItemOptions
                {
                    PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), 
                        Currency = "vnd",
                        ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                            Images = new List<string> { item.Product.ImageUrl }
                        }
                    },
                    Quantity = item.Quantity
                }).ToList(),
                Mode = "payment",
                SuccessUrl = $"{domain}/Cart/CheckoutSuccess",
                CancelUrl = $"{domain}/Cart/Cart"
            };

            var service = new Stripe.Checkout.SessionService();
            var session = service.Create(options);

            return Json(new { id = session.Id });
        }

        public IActionResult CheckoutSuccess()
        {
            TempData["Success"] = "Thanh toán thành công!";
            HttpContext.Session.Remove("Cart"); 
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            try
            {
                await _mediator.Send(new RemoveFromCartCommand(id));
                TempData["Success"] = "Sản phẩm đã được xóa khỏi giỏ hàng.";
                return RedirectToAction("Cart");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã xảy ra lỗi khi xóa sản phẩm: {ex.Message}";
                return RedirectToAction("Cart");
            }
        }

        [HttpPost]
        public IActionResult ApplyCoupon(string couponCode, decimal shippingCost)
        {
            if (string.IsNullOrEmpty(couponCode))
            {
                return Json(new { success = false, message = "Mã coupon không được để trống." });
            }

            var coupon = _context.Coupons.FirstOrDefault(c => c.Code == couponCode && c.IsActive && c.ExpiryDate >= DateTime.Now);
            if (coupon == null)
            {
                return Json(new { success = false, message = "Mã coupon không hợp lệ hoặc đã hết hạn." });
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<OrderItems>>("Cart") ?? new List<OrderItems>();
            if (!cart.Any())
            {
                return Json(new { success = false, message = "Giỏ hàng của bạn đang trống." });
            }

            var total = cart.Sum(c => c.Price * c.Quantity);

            var totalWithShipping = total + shippingCost;

            var discountedTotal = Math.Max(0, totalWithShipping - coupon.DiscountAmount);

            TempData["AppliedCoupon"] = couponCode;

            return Json(new
            {
                success = true,
                discountedTotal = discountedTotal.ToString("N0"),
                shippingCost = shippingCost.ToString("N0"),
                message = $"Coupon đã được áp dụng! Bạn đã tiết kiệm {Math.Min(coupon.DiscountAmount, totalWithShipping):N0} VND."
            });
        }
    }
}