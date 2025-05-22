using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Models;

namespace StoreManagement.Controllers // Added namespace to resolve CS0246
{
    public class SupplierController : Controller // Fixed CS0246 by defining the class properly
    {
        private readonly StoreDbContext _context;
        private readonly SupplierService _service;

        public SupplierController(StoreDbContext context) // Fixed CS1001 by ensuring proper constructor definition
        {
            _context = context;
            _service = new SupplierService(_context);
        }

        public async Task<IActionResult> Index()
        {
            var suppliers = await _service.GetAllAsync();
            return View(suppliers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _service.GetByIdAsync(id.Value);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(supplier);
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _service.FindAsync(id.Value);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            if (id != supplier.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var updated = await _service.UpdateAsync(supplier);
                if (!updated) return NotFound();
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _service.GetByIdAsync(id.Value);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }



        // Clase interna SupplierService
        private class SupplierService
        {
            private readonly StoreDbContext _context;

            public SupplierService(StoreDbContext context)
            {
                _context = context;
            }

            public async Task<List<Supplier>> GetAllAsync()
            {
                return await _context.Suppliers.OrderBy(s => s.Name).ToListAsync();
            }

            public async Task<Supplier?> GetByIdAsync(int id)
            {
                return await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
            }

            public async Task<Supplier?> FindAsync(int id)
            {
                return await _context.Suppliers.FindAsync(id);
            }

            public async Task CreateAsync(Supplier supplier)
            {
                var existingIds = await _context.Suppliers.Select(s => s.Id).ToListAsync();
                int newId = 1;
                while (existingIds.Contains(newId)) newId++;
                supplier.Id = newId;

                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();
            }

            public async Task<bool> UpdateAsync(Supplier supplier)
            {
                if (!_context.Suppliers.Any(s => s.Id == supplier.Id)) return false;

                _context.Suppliers.Update(supplier);
                await _context.SaveChangesAsync();
                return true;
            }

            public async Task DeleteAsync(int id)
            {
                var supplier = await _context.Suppliers.FindAsync(id);
                if (supplier != null)
                {
                    _context.Suppliers.Remove(supplier);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
