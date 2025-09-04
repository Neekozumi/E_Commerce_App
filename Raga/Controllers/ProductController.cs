using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Afrodite.Data;
using Afrodite.Models; 

public class ProductController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }


    public IActionResult Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return View("SearchResults", Enumerable.Empty<Products>()); 
        }

        var results = _context.Products
            .Where(p => p.Name.Contains(query) || p.Description.Contains(query))
            .ToList();

        return View("SearchResults", results);
    }
}