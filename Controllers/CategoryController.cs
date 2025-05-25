using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Models;

namespace StoreManagement.Controllers
{
    public class CategoryController : Controller
    {
        private readonly StoreDbContext _context;
        private readonly CategoryService _service;

        public CategoryController(StoreDbContext context)
        {
            _context = context;
            _service = new CategoryService(_context);
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var categories = await _service.GetAllAsync();
            return View(categories);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _service.GetByIdAsync(id.Value);
            if (category == null) return NotFound();

            return View(category);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _service.FindAsync(id.Value);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var success = await _service.UpdateAsync(category);
                if (!success) return NotFound();

                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _service.GetByIdAsync(id.Value);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Servicio interno para manejo de datos de Category
        private class CategoryService
        {
            private readonly StoreDbContext _context;

            public CategoryService(StoreDbContext context)
            {
                _context = context;
            }

            public async Task<List<Category>> GetAllAsync()
            {
                return await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }

            public async Task<Category?> GetByIdAsync(int id)
            {
                return await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }

            public async Task<Category?> FindAsync(int id)
            {
                return await _context.Categories.FindAsync(id);
            }

            public async Task CreateAsync(Category category)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            public async Task<bool> UpdateAsync(Category category)
            {
                if (!_context.Categories.Any(c => c.Id == category.Id))
                    return false;

                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                return true;
            }

            public async Task DeleteAsync(int id)
            {
                var category = await _context.Categories.FindAsync(id);
                if (category != null)
                {
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
