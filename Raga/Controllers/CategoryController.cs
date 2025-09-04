namespace Afrodite.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ListCategory()
        {
            var categories = _context.Categories
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    Products = c.Products.ToList() 
                })
                .ToList();

            return View(categories);
        }

        public IActionResult Create()
        {
            var model = new CategoryDto();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryDto categoryDto)
        {
            if (ModelState.IsValid)
            {
                var category = new Categories
                {
                    Name = categoryDto.Name,
                    Description = categoryDto.Description
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ListCategory));
            }

            return View(categoryDto);
        }

        public IActionResult Edit(int id)
        {
            var category = _context.Categories
                .Where(c => c.CategoryId == id)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description
                })
                .FirstOrDefault();

            if (category == null)
            {
                return NotFound();
            }

            return View(category); 
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CategoryDto categoryDto)
        {
            if (ModelState.IsValid)
            {
                var category = await _context.Categories.FindAsync(categoryDto.CategoryId);

                if (category == null)
                {
                    return NotFound();
                }

                category.Name = categoryDto.Name; 
                category.Description = categoryDto.Description;

                await _context.SaveChangesAsync(); 
                return RedirectToAction(nameof(ListCategory));
            }

            return View(categoryDto); 
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ListCategory));
        }
    }
}