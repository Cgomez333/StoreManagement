using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Models;

namespace StoreManagement.Controllers
{
    public class InventoryMovementController : Controller
    {
        private readonly StoreDbContext _context;
        private readonly InventoryMovementService _service;

        public InventoryMovementController(StoreDbContext context)
        {
            _context = context;
            _service = new InventoryMovementService(_context);
        }

        // GET: InventoryMovement
        public async Task<IActionResult> Index()
        {
            var movements = await _service.GetAllAsync();
            return View(movements);
        }

        // GET: InventoryMovement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var movement = await _service.GetByIdAsync(id.Value);
            if (movement == null) return NotFound();
            return View(movement);
        }

        // GET: InventoryMovement/Create
        public async Task<IActionResult> Create()
        {
            await PopulateProductsDropDown();
            // inicializamos fecha con ahora UTC
            return View(new InventoryMovement { Date = DateTime.UtcNow });
        }

        // POST: InventoryMovement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryMovement movement)
        {
            if (!ModelState.IsValid)
            {
                await PopulateProductsDropDown(movement.ProductId);
                return View(movement);
            }

            // asegurar que Date tenga Kind=UTC
            movement.Date = movement.Date.ToUniversalTime();

            await _service.CreateAsync(movement);
            return RedirectToAction(nameof(Index));
        }

        // GET: InventoryMovement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var movement = await _service.GetByIdAsync(id.Value);
            if (movement == null) return NotFound();
            await PopulateProductsDropDown(movement.ProductId);
            return View(movement);
        }

        // POST: InventoryMovement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InventoryMovement movement)
        {
            if (id != movement.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                await PopulateProductsDropDown(movement.ProductId);
                return View(movement);
            }

            // convertir a UTC antes de guardar
            movement.Date = movement.Date.ToUniversalTime();

            var success = await _service.UpdateAsync(movement);
            if (!success) return NotFound();
            return RedirectToAction(nameof(Index));
        }

        // GET: InventoryMovement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var movement = await _service.GetByIdAsync(id.Value);
            if (movement == null) return NotFound();
            return View(movement);
        }

        // POST: InventoryMovement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateProductsDropDown(object selectedValue = null)
        {
            var products = await _context.Products
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();
            ViewBag.ProductId = new SelectList(products, "Id", "Name", selectedValue);
        }

        private class InventoryMovementService
        {
            private readonly StoreDbContext _context;

            public InventoryMovementService(StoreDbContext context)
            {
                _context = context;
            }

            public async Task<List<InventoryMovement>> GetAllAsync()
                => await _context.InventoryMovements
                    .Include(m => m.Product)
                    .OrderByDescending(m => m.Date)
                    .ToListAsync();

            public async Task<InventoryMovement?> GetByIdAsync(int id)
                => await _context.InventoryMovements
                    .Include(m => m.Product)
                    .FirstOrDefaultAsync(m => m.Id == id);

            public async Task CreateAsync(InventoryMovement movement)
            {
                _context.InventoryMovements.Add(movement);
                await _context.SaveChangesAsync();
            }

            public async Task<bool> UpdateAsync(InventoryMovement movement)
            {
                if (!await _context.InventoryMovements.AnyAsync(m => m.Id == movement.Id))
                    return false;
                _context.InventoryMovements.Update(movement);
                await _context.SaveChangesAsync();
                return true;
            }

            public async Task DeleteAsync(int id)
            {
                var m = await _context.InventoryMovements.FindAsync(id);
                if (m != null)
                {
                    _context.InventoryMovements.Remove(m);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
