using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Models;

namespace StoreManagement.Controllers
{
    public class ProductController : Controller
    {
        private readonly StoreDbContext _context;
        private readonly ProductService _service;
        private const int PageSize = 10;

        public ProductController(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchName, int? searchCategory, int? searchSupplier, int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
                query = query.Where(p => p.Name.Contains(searchName));

            if (searchCategory.HasValue)
                query = query.Where(p => p.CategoryId == searchCategory.Value);

            if (searchSupplier.HasValue)
                query = query.Where(p => p.SupplierId == searchSupplier.Value);

            int totalItems = await query.CountAsync();
            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.SearchName = searchName;
            ViewBag.SearchCategory = searchCategory;
            ViewBag.SearchSupplier = searchSupplier;

            return View(products);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var product = await _service.GetByIdAsync(id.Value);
            if (product == null) return NotFound();
            return View(product);
        }

        public IActionResult Create()
        {
            LoadViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(product);
                return RedirectToAction(nameof(Index));
            }
            LoadViewBags();
            return View(product);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _service.FindAsync(id.Value);
            if (product == null) return NotFound();
            LoadViewBags(product.CategoryId, product.SupplierId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var success = await _service.UpdateAsync(product);
                if (!success) return NotFound();
                return RedirectToAction(nameof(Index));
            }

            LoadViewBags(product.CategoryId, product.SupplierId);
            return View(product);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _service.GetByIdAsync(id.Value);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private void LoadViewBags(int? categoryId = null, int? supplierId = null)
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", categoryId);
            ViewBag.SupplierId = new SelectList(_context.Suppliers, "Id", "Name", supplierId);
        }

        // Clase interna ProductService
        private class ProductService
        {
            private readonly StoreDbContext _context;

            public ProductService(StoreDbContext context)
            {
                _context = context;
            }

            public async Task<List<Product>> GetAllAsync()
            {
                return await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .OrderBy(p => p.Id)
                    .ToListAsync();
            }

            public async Task<Product?> GetByIdAsync(int id)
            {
                return await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }

            public async Task<Product?> FindAsync(int id)
            {
                return await _context.Products.FindAsync(id);
            }

            public async Task CreateAsync(Product product)
            {
                var existingIds = await _context.Products.Select(p => p.Id).ToListAsync();
                int newId = 1;
                while (existingIds.Contains(newId)) newId++;
                product.Id = newId;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }

            public async Task<bool> UpdateAsync(Product product)
            {
                if (!_context.Products.Any(p => p.Id == product.Id)) return false;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return true;
            }

            public async Task DeleteAsync(int id)
            {
                var product = await _context.Products.FindAsync(id);
                if (product != null)
                {
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
