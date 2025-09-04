namespace Afrodite.Controllers
{
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BrandController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ListBrand()
        {
            var brand = _context.Brand
                .Select(b => new BrandDto
                {
                    BrandId = b.BrandId,
                    Name = b.Name,
                })
                .ToList();

            return View(brand);
        }

        public IActionResult Create()
        {
            var model = new BrandDto();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BrandDto BrandDto)
        {
            if (ModelState.IsValid)
            {
                var Brand = new Brand
                {
                    Name = BrandDto.Name,
                };

                _context.Brand.Add(Brand);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ListBrand));
            }

            return View(BrandDto);
        }

        public IActionResult Edit(int id)
        {
            var Brand = _context.Brand
                .Where(b => b.BrandId == id)
                .Select(b => new BrandDto
                {
                    BrandId = b.BrandId,
                    Name = b.Name,
                })
                .FirstOrDefault();

            if (Brand == null)
            {
                return NotFound();
            }

            return View(Brand); 
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BrandDto BrandDto)
        {
            if (ModelState.IsValid)
            {
                var Brand = await _context.Brand.FindAsync(BrandDto.BrandId);

                if (Brand == null)
                {
                    return NotFound();
                }

                Brand.Name = BrandDto.Name; 

                await _context.SaveChangesAsync(); 
                return RedirectToAction(nameof(ListBrand));
            }

            return View(BrandDto); 
        }

        public async Task<IActionResult> Delete(int id)
        {
            var Brand = await _context.Brand.FindAsync(id);

            if (Brand == null)
            {
                return NotFound();
            }

            _context.Brand.Remove(Brand);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ListBrand));
        }
    }
}