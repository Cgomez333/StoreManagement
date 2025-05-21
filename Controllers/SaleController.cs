using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Models;

namespace StoreManagement.Controllers
{
    public class SaleController : Controller
    {
        private readonly StoreDbContext _context;
        private readonly SaleService _service;
        private const int PageSize = 5;

        public SaleController(StoreDbContext context)
        {
            _context = context;
            _service = new SaleService(_context);
        }

        // Index con paginación y búsqueda por fecha o cliente opcional
        public async Task<IActionResult> Index(string? searchCustomerName, DateTime? searchDate, int page = 1)
        {
            var query = _context.Sales
                .Include(s => s.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchCustomerName))
            {
                query = query.Where(s => s.Customer != null && s.Customer.Name.Contains(searchCustomerName));
            }

            if (searchDate.HasValue)
            {
                // Convertir a UTC para que EF Core y Npgsql no falle
                var utcDate = DateTime.SpecifyKind(searchDate.Value.Date, DateTimeKind.Utc);

                query = query.Where(s => s.Date >= utcDate && s.Date < utcDate.AddDays(1));
            }

            int totalItems = await query.CountAsync();

            var sales = await query
                .OrderByDescending(s => s.Date)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.SearchCustomerName = searchCustomerName;
            ViewBag.SearchDate = searchDate?.ToString("yyyy-MM-dd");

            return View(sales);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _service.GetByIdAsync(id.Value);
            if (sale == null) return NotFound();

            return View(sale);
        }

        public IActionResult Create()
        {
            var sale = new Sale();
            LoadViewBags();
            return View(sale); // No enviar null
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sale sale)
        {
            if (!ModelState.IsValid)
            {
                LoadViewBags(sale.CustomerId);
                return View(sale);
            }

            // 1) Verificar stock
            foreach (var detail in sale.SaleDetails)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == detail.ProductId);

                if (product == null)
                {
                    ModelState.AddModelError("", $"Producto con Id {detail.ProductId} no encontrado.");
                    break;
                }

                if (detail.Quantity > product.Stock)
                {
                    ModelState.AddModelError("",
                        $"No hay suficiente stock de «{product.Name}». " +
                        $"Disponibles: {product.Stock}, solicitados: {detail.Quantity}.");
                    break;
                }
            }

            if (!ModelState.IsValid)
            {
                LoadViewBags(sale.CustomerId);
                return View(sale);
            }

            // 2) Crear la venta y actualizar stock
            await _service.CreateAsync(sale);
            return RedirectToAction(nameof(Index));
        }



        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _service.GetByIdWithDetailsAsync(id.Value);
            if (sale == null) return NotFound();

            LoadViewBags(sale.CustomerId);
            return View(sale);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Sale sale)
        {
            if (id != sale.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                LoadViewBags(sale.CustomerId);
                return View(sale);
            }

            // 1) Cargo la venta original con sus detalles
            var original = await _context.Sales
                .Include(s => s.SaleDetails)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (original == null) return NotFound();

            // 2) Actualizo campos de cabecera
            original.Date = sale.Date.ToUniversalTime();
            original.CustomerId = sale.CustomerId;

            // 3) Elimino los detalles que el usuario borró
            var toRemove = original.SaleDetails
                .Where(od => !sale.SaleDetails.Any(d => d.Id == od.Id))
                .ToList();
            _context.SaleDetails.RemoveRange(toRemove);

            // 4) Recorro los detalles enviados
            foreach (var d in sale.SaleDetails)
            {
                if (d.Id == 0)
                {
                    // nuevo
                    d.Subtotal = d.Quantity * d.UnitPrice;
                    original.SaleDetails.Add(d);
                }
                else
                {
                    // existente: actualizo
                    var existingDetail = original.SaleDetails.First(x => x.Id == d.Id);
                    existingDetail.Quantity = d.Quantity;
                    existingDetail.UnitPrice = d.UnitPrice;
                    existingDetail.Subtotal = d.Quantity * d.UnitPrice;
                }
            }

            // 5) Recalculo total y salvo
            original.RecalculateTotal();
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _service.GetByIdAsync(id.Value);
            if (sale == null) return NotFound();

            return View(sale);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private void LoadViewBags(int? customerId = null)
        {
            ViewBag.CustomerId = new SelectList(
                _context.Customers.OrderBy(c => c.Name),
                "Id", "Name", customerId);

            // PARA EL dropdown HTML:
            ViewBag.ProductList = _context.Products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                })
                .ToList();

            // PARA EL JS:
            ViewBag.ProductsJson = _context.Products
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, p.Name, p.Price })
                .ToList();
        }



        // Servicio interno para manejo de datos Sale
        private class SaleService
        {
            private readonly StoreDbContext _context;

            public SaleService(StoreDbContext context)
            {
                _context = context;
            }

            public async Task<List<Sale>> GetAllAsync()
            {
                return await _context.Sales
                    .Include(s => s.Customer)
                    .OrderByDescending(s => s.Date)
                    .ToListAsync();
            }

            public async Task<Sale?> GetByIdAsync(int id)
            {
                return await _context.Sales
                    .Include(s => s.Customer)
                    .Include(s => s.SaleDetails)
                    .FirstOrDefaultAsync(s => s.Id == id);
            }

            public async Task<Sale?> GetByIdWithDetailsAsync(int id)
            {
                return await _context.Sales
                    .Include(s => s.Customer)
                    .Include(s => s.SaleDetails)
                    .FirstOrDefaultAsync(s => s.Id == id);
            }


            public async Task<Sale?> FindAsync(int id)
            {
                return await _context.Sales.FindAsync(id);
            }

            public async Task CreateAsync(Sale sale)
            {
                using var tx = await _context.Database.BeginTransactionAsync();

                // Recalcular todos los subtotales, total, y ajustar stock
                foreach (var detail in sale.SaleDetails)
                {
                    // 1) Calcular subtotal
                    detail.Subtotal = detail.Quantity * detail.UnitPrice;

                    // 2) Restar stock
                    var prod = await _context.Products
                        .FirstAsync(p => p.Id == detail.ProductId);

                    prod.Stock -= detail.Quantity;
                    _context.Products.Update(prod);
                }

                sale.RecalculateTotal();
                sale.Date = sale.Date.ToUniversalTime();

                // 3) Finalmente, persistir
                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();
            }


            public async Task<bool> UpdateAsync(Sale sale)
            {
                if (!_context.Sales.Any(s => s.Id == sale.Id)) return false;

                sale.Date = sale.Date.ToUniversalTime();
                _context.Sales.Update(sale);
                await _context.SaveChangesAsync();
                return true;
            }

            public async Task DeleteAsync(int id)
            {
                var sale = await _context.Sales.FindAsync(id);
                if (sale != null)
                {
                    _context.Sales.Remove(sale);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
