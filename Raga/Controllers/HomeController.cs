using Afrodite.Models; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Afrodite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; 
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var products = _context.Products
                .Select(p => new ManageProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    ShortDescription = p.ShortDescription
                })
                .ToList(); 

            return View(products); 
        }

        public IActionResult Sanphambanchaynhat()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        public IActionResult ProductDetail(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Store()
        {
            var products = _context.Products
                .Select(p => new ManageProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    ShortDescription = p.ShortDescription
                })
                .ToList(); 

            return View(products); 
        }

        public IActionResult Kuromi()
        {
            return View();
        }

        public IActionResult Minikeyboard()
        {
            return View();
        }

        public IActionResult PC()
        {
            return View();
        }
    }
}