
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using StoreManagement.Models;

    namespace StoreManagement.Controllers
    {
        public class ProductController : Controller
        {
            private readonly InventoryDbContext _context;

            public ProductController(InventoryDbContext context)
            {
                _context = context;
            }

            // GET: Product
            public async Task<IActionResult> Index()
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .ToListAsync();
                return View(products);
            }

            // GET: Product/Details/5
            public async Task<IActionResult> Details(int? id)
            {
                if (id == null) return NotFound();

                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null) return NotFound();

                return View(product);
            }

            // GET: Product/Create
            public IActionResult Create()
            {
                return View();
            }

            // POST: Product/Create
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(Product product)
            {
                if (ModelState.IsValid)
                {
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                return View(product);
            }

            // GET: Product/Edit/5
            public async Task<IActionResult> Edit(int? id)
            {
                if (id == null) return NotFound();

                var product = await _context.Products.FindAsync(id);
                if (product == null) return NotFound();

                return View(product);
            }

            // POST: Product/Edit/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, Product product)
            {
                if (id != product.Id) return NotFound();

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(product);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_context.Products.Any(p => p.Id == id))
                            return NotFound();

                        throw;
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View(product);
            }

            // GET: Product/Delete/5
            public async Task<IActionResult> Delete(int? id)
            {
                if (id == null) return NotFound();

                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null) return NotFound();

                return View(product);
            }

            // POST: Product/Delete/5
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var product = await _context.Products.FindAsync(id);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }
    }

