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

        public async Task<IActionResult> Index()
        {
            var movements = await _service.GetAllAsync();
            return View(movements);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var movement = await _service.GetByIdAsync(id.Value);
            if (movement == null) return NotFound();
            return View(movement);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.ProductId = new SelectList(await _context.Products.ToListAsync(), "Id", "Name");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryMovement movement)
        {
            Console.WriteLine($"ProductId recibido: {movement.ProductId}");
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(movement);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ProductId = new SelectList(await _context.Products.ToListAsync(), "Id", "Name", movement.ProductId);
            TempData["Error"] = "El modelo no es válido.";
            return View(movement);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var movement = await _service.GetByIdAsync(id.Value);
            if (movement == null) return NotFound();
            return View(movement);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private class InventoryMovementService
        {
            private readonly StoreDbContext _context;

            public InventoryMovementService(StoreDbContext context)
            {
                _context = context;
            }

            public async Task<List<InventoryMovement>> GetAllAsync()
            {
                return await _context.InventoryMovements
                    .Include(m => m.Product)
                    .OrderByDescending(m => m.Date)
                    .ToListAsync();
            }

            public async Task<InventoryMovement?> GetByIdAsync(int id)
            {
                return await _context.InventoryMovements
                    .Include(m => m.Product)
                    .FirstOrDefaultAsync(m => m.Id == id);
            }

            public async Task CreateAsync(InventoryMovement movement)
            {
                var existingIds = await _context.InventoryMovements.Select(m => m.Id).ToListAsync();
                int newId = 1;
                while (existingIds.Contains(newId)) newId++;
                movement.Id = newId;
                movement.Date = DateTime.UtcNow;

                _context.InventoryMovements.Add(movement);
                await _context.SaveChangesAsync();
            }

            public async Task DeleteAsync(int id)
            {
                var movement = await _context.InventoryMovements.FindAsync(id);
                if (movement != null)
                {
                    _context.InventoryMovements.Remove(movement);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
