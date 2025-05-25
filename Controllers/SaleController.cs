using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Models;

namespace StoreManagement.Controllers
{
    public class SaleController : Controller
    {
        private readonly StoreDbContext _context;
        private const int PageSize = 5;

        public SaleController(StoreDbContext context)
        {
            _context = context;
        }

        // Index con paginación y búsqueda
        public async Task<IActionResult> Index(string? searchCustomerName, DateTime? searchDate, int page = 1)
        {
            var query = _context.Sales.Include(s => s.Customer).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchCustomerName))
                query = query.Where(s => s.Customer != null && s.Customer.Name.Contains(searchCustomerName));

            if (searchDate.HasValue)
            {
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

        // Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(s => s.Id == id.Value);

            if (sale == null) return NotFound();
            return View(sale);
        }

        // GET Create
        public IActionResult Create()
        {
            LoadViewBags();
            return View(new Sale { Date = DateTime.UtcNow });
        }

        // POST Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sale sale)
        {
            // stock y validación modelos
            foreach (var detail in sale.SaleDetails)
            {
                var prod = await _context.Products.FindAsync(detail.ProductId);
                if (prod == null)
                {
                    ModelState.AddModelError("", $"Producto {detail.ProductId} no existe.");
                    break;
                }
                if (detail.Quantity > prod.Stock)
                {
                    ModelState.AddModelError("", $"Stock insuficiente de «{prod.Name}». Disponible: {prod.Stock}.");
                    break;
                }
            }

            if (!ModelState.IsValid)
            {
                LoadViewBags(sale.CustomerId);
                return View(sale);
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            // rebaja de stock y cálculo de subtotales
            foreach (var detail in sale.SaleDetails)
            {
                var prod = await _context.Products.FindAsync(detail.ProductId);
                prod.Stock -= detail.Quantity;
                detail.Subtotal = detail.Quantity * detail.UnitPrice;
            }
            sale.RecalculateTotal();
            sale.Date = sale.Date.ToUniversalTime();

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET Edit
        // GET: Sale/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales
                .Include(s => s.SaleDetails!)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(s => s.Id == id.Value);

            if (sale == null) return NotFound();

            sale.SaleDetails ??= new List<SaleDetail>();

            LoadViewBags(sale.CustomerId);
            return View(sale);
        }

        // POST Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Sale submitted)
        {
            if (id != submitted.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                LoadViewBags(submitted.CustomerId);
                return View(submitted);
            }

            var original = await _context.Sales
                .Include(s => s.SaleDetails)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (original == null) return NotFound();

            using var tx = await _context.Database.BeginTransactionAsync();

            // 1) Restaurar stock de los detalles originales
            foreach (var od in original.SaleDetails)
            {
                var prod = await _context.Products.FindAsync(od.ProductId);
                prod.Stock += od.Quantity;
            }

            // 2) Eliminar detalles borrados
            var toRemove = original.SaleDetails
                .Where(od => !submitted.SaleDetails.Any(d => d.Id == od.Id))
                .ToList();
            _context.SaleDetails.RemoveRange(toRemove);

            // 3) Aplicar nuevos y actualizados
            foreach (var d in submitted.SaleDetails)
            {
                var prod = await _context.Products.FindAsync(d.ProductId);
                if (d.Id == 0)
                {
                    // nuevo
                    d.Subtotal = d.Quantity * d.UnitPrice;
                    original.SaleDetails.Add(d);
                }
                else
                {
                    // existente
                    var exist = original.SaleDetails.First(x => x.Id == d.Id);
                    exist.Quantity = d.Quantity;
                    exist.UnitPrice = d.UnitPrice;
                    exist.Subtotal = d.Quantity * d.UnitPrice;
                }
                // descontar stock
                prod.Stock -= d.Quantity;
            }

            // 4) Actualizar cabecera
            original.Date = submitted.Date.ToUniversalTime();
            original.CustomerId = submitted.CustomerId;
            original.RecalculateTotal();

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var sale = await _context.Sales
                .Include(s => s.SaleDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (sale == null) return NotFound();
            return View(sale);
        }

        // POST Delete
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.SaleDetails)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (sale == null) return NotFound();

            using var tx = await _context.Database.BeginTransactionAsync();
            // restaurar stock
            foreach (var d in sale.SaleDetails)
            {
                var prod = await _context.Products.FindAsync(d.ProductId);
                prod.Stock += d.Quantity;
            }

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return RedirectToAction(nameof(Index));
        }

        // Helpers
        private void LoadViewBags(int? customerId = null)
        {
            ViewBag.CustomerId = new SelectList(
                _context.Customers.OrderBy(c => c.Name), "Id", "Name", customerId);

            ViewBag.ProductList = _context.Products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            ViewBag.ProductsJson = _context.Products
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, p.Name, p.Price })
                .ToList();
        }
    }
}
