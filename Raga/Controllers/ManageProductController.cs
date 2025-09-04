using Afrodite.Models;
using Afrodite.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
namespace Afrodite.Controllers
{
    public class ManageProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ManageProductController(
            ApplicationDbContext context, 
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        
        [HttpGet]
        [Authorize(Roles = "AdminTổng")]
        public IActionResult ManageProduct()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Select(p => new ManageProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    ShortDescription = p.ShortDescription,
                    Price = p.Price
                })
                .ToList();

            return View(products);
        }

        [HttpGet]
        public IActionResult CreateProduct()
        {
            var model = new CreateProductDto
            {
                Categories = _context.Categories
                    .Select(c => new CategoryDto
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name
                    })
                    .ToList(),
                Brand = _context.Brand
                    .Select(b => new BrandDto
                    {
                        BrandId = b.BrandId,
                        Name = b.Name
                    })
                    .ToList()
            };

            return View(model);
        }

        public class ProductFactory
        {
            public static Products CreateProduct(CreateProductDto productDto, string imageUrl)
            {
                return new Products
                {
                    Name = productDto.Name,
                    Price = productDto.Price,
                    ShortDescription = productDto.ShortDescription,
                    Description = productDto.Description,
                    Stock = productDto.Stock,
                    CategoryId = productDto.CategoryId,
                    BrandId = productDto.BrandId,
                    ImageUrl = imageUrl
                };
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductDto productDto, IFormFile ImageUrl)
        {
            if (productDto == null)
            {
                return BadRequest("Dữ liệu sản phẩm không hợp lệ");
            }

            try
            {
                string imageUrl = null;

                if (ImageUrl != null && ImageUrl.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageUrl.FileName;
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
                    Directory.CreateDirectory(uploadsFolder);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageUrl.CopyToAsync(fileStream);
                    }

                    imageUrl = $"/uploads/products/{uniqueFileName}";
                }

                var product = ProductFactory.CreateProduct(productDto, imageUrl);

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Sản phẩm đã được tạo thành công!";
                return RedirectToAction(nameof(ManageProduct));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo sản phẩm: " + ex.Message);
                productDto.Categories = _context.Categories
                    .Select(c => new CategoryDto { CategoryId = c.CategoryId, Name = c.Name })
                    .ToList();
                productDto.Brand = _context.Brand
                    .Select(b => new BrandDto { BrandId = b.BrandId, Name = b.Name })
                    .ToList();
                return View(productDto);
            }
        }

        [HttpGet]
        public IActionResult ProductDetail(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Select(p => new ManageProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    ShortDescription = p.ShortDescription,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    Categories = new CategoryDto 
                    { 
                        CategoryId = p.Category.CategoryId, 
                        Name = p.Category.Name 
                    },
                    Brand = new BrandDto 
                    { 
                        BrandId = p.Brand.BrandId, 
                        Name = p.Brand.Name 
                    }
                })
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            var model = new CreateProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                Categories = _context.Categories
                    .Select(c => new CategoryDto
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name
                    })
                    .ToList(),
                Brand = _context.Brand
                    .Select(b => new BrandDto
                    {
                        BrandId = b.BrandId,
                        Name = b.Name
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(CreateProductDto productDto, IFormFile ImageUrl)
        {
            if (productDto == null)
            {
                return BadRequest("Dữ liệu sản phẩm không hợp lệ");
            }

            try 
            {
                var existingProduct = await _context.Products.FindAsync(productDto.ProductId);
                
                if (existingProduct == null)
                {
                    return NotFound();
                }

                existingProduct.Name = productDto.Name;
                existingProduct.Price = productDto .Price;
                existingProduct.ShortDescription = productDto.ShortDescription;
                existingProduct.Description = productDto.Description;
                existingProduct.Stock = productDto.Stock;
                existingProduct.CategoryId = productDto.CategoryId;
                existingProduct.BrandId = productDto.BrandId;

                if (ImageUrl != null && ImageUrl.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageUrl.FileName;
                    
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
                    
                    Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageUrl.CopyToAsync(fileStream);
                    }

                    existingProduct.ImageUrl = $"/uploads/products/{uniqueFileName}";
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công!";

                return RedirectToAction(nameof(ManageProduct));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật sản phẩm: " + ex.Message);

                productDto.Categories = _context.Categories
                    .Select(c => new CategoryDto
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name
                    })
                    .ToList();

                productDto.Brand = _context.Brand
                    .Select(b => new BrandDto
                    {
                        BrandId = b.BrandId,
                        Name = b.Name
                    })
                    .ToList();

                return View(productDto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Sản phẩm đã được xóa thành công!";
            return RedirectToAction(nameof(ManageProduct));
        }
    }
}