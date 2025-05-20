using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Models;

namespace StoreManagement.Controllers
{
    public class CustomerController : Controller
    {
        private readonly StoreDbContext _context;
        private readonly CustomerService _service;

        public CustomerController(StoreDbContext context)
        {
            _context = context;
            _service = new CustomerService(context);
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _service.GetAllAsync();
            return View(customers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _service.GetByIdAsync(id.Value);
            if (customer == null) return NotFound();

            return View(customer);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _service.FindAsync(id.Value);
            if (customer == null) return NotFound();

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var success = await _service.UpdateAsync(customer);
                if (!success) return NotFound();

                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _service.GetByIdAsync(id.Value);
            if (customer == null) return NotFound();

            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Servicio interno para manejo de datos de Customer
        private class CustomerService
        {
            private readonly StoreDbContext _context;

            public CustomerService(StoreDbContext context)
            {
                _context = context;
            }

            public async Task<List<Customer>> GetAllAsync()
            {
                return await _context.Customers
                    .Include(c => c.Sales)
                    .OrderBy(c => c.Id)
                    .ToListAsync();
            }

            public async Task<Customer?> GetByIdAsync(int id)
            {
                return await _context.Customers
                    .Include(c => c.Sales)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }

            public async Task<Customer?> FindAsync(int id)
            {
                return await _context.Customers.FindAsync(id);
            }

            public async Task CreateAsync(Customer customer)
            {
                var existingIds = await _context.Customers.Select(c => c.Id).ToListAsync();
                int newId = 1;
                while (existingIds.Contains(newId)) newId++;
                customer.Id = newId;

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            public async Task<bool> UpdateAsync(Customer customer)
            {
                if (!_context.Customers.Any(c => c.Id == customer.Id)) return false;

                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
                return true;
            }

            public async Task DeleteAsync(int id)
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer != null)
                {
                    _context.Customers.Remove(customer);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
