using Microsoft.AspNetCore.Mvc;
using Afrodite.Models;
using System.Linq;

namespace Afrodite.Controllers
{
    public class CouponsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CouponsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var coupons = _context.Coupons.ToList();
            return View("Coupons", coupons); 
        }

        [HttpPost]
        public IActionResult AddCoupon(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                _context.Coupons.Add(coupon);
                _context.SaveChanges();
                TempData["Success"] = "Coupon added successfully.";
            }
            return RedirectToAction("Index");
        }

        public IActionResult EditCoupon(int id)
        {
            var coupon = _context.Coupons.FirstOrDefault(c => c.CouponId == id);
            if (coupon == null)
            {
                TempData["Error"] = "Coupon not found.";
                return RedirectToAction("Index");
            }
            return View("EditCoupon", coupon);
        }

        [HttpPost]
        public IActionResult UpdateCoupon(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                var existingCoupon = _context.Coupons.FirstOrDefault(c => c.CouponId == coupon.CouponId);
                if (existingCoupon != null)
                {
                    existingCoupon.Code = coupon.Code;
                    existingCoupon.DiscountAmount = coupon.DiscountAmount;
                    existingCoupon.ExpiryDate = coupon.ExpiryDate;
                    existingCoupon.IsActive = coupon.IsActive;

                    _context.SaveChanges();
                    TempData["Success"] = "Coupon updated successfully.";
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteCoupon(int id)
        {
            var coupon = _context.Coupons.FirstOrDefault(c => c.CouponId == id);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                _context.SaveChanges();
                TempData["Success"] = "Coupon deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Coupon not found.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ToggleCouponStatus(int id)
        {
            var coupon = _context.Coupons.FirstOrDefault(c => c.CouponId == id);
            if (coupon == null)
            {
                TempData["Error"] = "Coupon not found.";
                return RedirectToAction("Index");
            }

            // Toggle the IsActive status
            coupon.IsActive = !coupon.IsActive;
            _context.SaveChanges();

            TempData["Success"] = $"Coupon status updated to {(coupon.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction("Index");
        }
    }
}
